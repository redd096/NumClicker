using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class HighScoreController : MonoBehaviour
{
    public string secretKey = "chiaveSegretissima"; // Edit this value and make sure it's the same as the one stored on the server
    [TextArea (1, 10)]
    public string addScoreURL = "http://numclickerofficial.altervista.org/numclicker_scores/addscore.php"; //il ? alla fine, serve per poter aggiungere dei valori
    [TextArea(1, 10)]
    public string highscoreURL = "http://numclickerofficial.altervista.org/numclicker_scores/display.php"; // come quando si chiama una funzione, passandole dei valori
    public Text Score_HighScore;
    public GameObject Scores_Layout;

    // remember to use StartCoroutine when calling this function!
    public IEnumerator PostScores(int id, string name, int scores, MenuScript ms)
    {
        Score_HighScore.gameObject.SetActive(true);

        Score_HighScore.text = "Add Scores...\n\n";

        //This connects to a server side php script that will add the name and score to a MySQL DB.
        // Supply it with a string representing the players name and the players score.
        string hash = Md5Sum(name + scores + secretKey);

        string post_url = addScoreURL + "?id=" + id + "&name=" + WWW.EscapeURL(name) + "&scores=" + scores + "&hash=" + hash;

        // Post the URL to the site and create a download object to get the result.
        WWW hs_post = new WWW(post_url);
        yield return hs_post; // Wait until the download is done

        if (hs_post.error != null)
        {
            Score_HighScore.text = "There was an error posting the high score.\nCheck your connection\n\n";
        }
        else
        {
            int temp_ID = id;
            if (temp_ID < 1)
            {
                int.TryParse(hs_post.text, out temp_ID);
                ms.ID = temp_ID;
                ms.control.dataPath.text = "ID: " + temp_ID + "\nData:\n" + Application.persistentDataPath;
            }

            StartCoroutine(GetScores(temp_ID));
        }
    }

    // Get the scores from the MySQL DB to display in a GUIText.
    // remember to use StartCoroutine when calling this function!
    public IEnumerator GetScores(int id)
    {
        Score_HighScore.text = "Loading Scores...\n\n";

        //passo un valore random ma non lo uso, solo per far sì che venga visto come un nuovo url e non venga presa la classifica nella cache
        string score_url = highscoreURL + "?id=" + id + "&t=" + Random.value;
        WWW hs_get = new WWW(score_url);
        yield return hs_get;

        if (hs_get.error != null)
        {
            Score_HighScore.text = "There was an error getting the high score.\nCheck your connection\n\n";
        }
        else
        {
            ShowScores(hs_get.text);
        }
    }

    private string Md5Sum(string strToEncrypt)
    {
        System.Text.UTF8Encoding ue = new System.Text.UTF8Encoding();
        byte[] bytes = ue.GetBytes(strToEncrypt);

        // encrypt bytes
        System.Security.Cryptography.MD5CryptoServiceProvider md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
        byte[] hashBytes = md5.ComputeHash(bytes);

        // Convert the encrypted bytes back to a string (base 16)
        string hashString = "";

        for (int i = 0; i < hashBytes.Length; i++)
        {
            hashString += System.Convert.ToString(hashBytes[i], 16).PadLeft(2, '0');
        }

        return hashString.PadLeft(32, '0');
    }

    private void ShowScores(string scores)
    {
        //divide le varie righe

        string[] rows = new string[100];
        string playerString = "";
        int lastRow = 0;
        int beginRow = 0;
        int lenghtRow = 0;

        for(int i = 0; i < scores.Length; i++)
        {
            lenghtRow += 1;

            if(scores[i] == '\n')
            {
                //una riga qalsiasi della classifica
                if (scores[i - 1] != '\n')
                {
                    rows[lastRow] = scores.Substring(beginRow, lenghtRow -1);
                    lastRow += 1;
                    beginRow = i +1;
                    lenghtRow = 0;
                }
                else
                {
                    //elimina le righe vuote, quelle in eccesso
                    if(lastRow < 100)
                    {
                        string[] temp_rows = rows;
                        rows = new string[lastRow];

                        for (int j = 0; j < rows.Length; j++)
                            rows[j] = temp_rows[j];
                    }

                    //l'ultima riga, quella del giocatore
                    playerString = scores.Substring(i + 1, scores.Length - (i + 1));
                }
            }
        }

        //divide le statistiche del giocatore
        
        Transform NextScores_transform = Scores_Layout.transform.parent.FindChild("NextScores");

        string[] s_player = SplitStatistics(playerString);

        //rimuove i vecchi punteggi per aggiungere quelli nuovi
        if(NextScores_transform.childCount > 0)
            for (int i = 0; i < NextScores_transform.childCount; i++)
                Destroy(NextScores_transform.GetChild(i).gameObject);

        //mostra il punteggio del giocatore
        Transform t_player = Instantiate(Scores_Layout).transform;
        t_player.SetParent(NextScores_transform, false);
        Text rank_text = t_player.FindChild("Rank_Strings").GetComponent<Text>();
        rank_text.color = new Color(1, 1, 0, 1);
        rank_text.text = s_player[0];
        Text name_text = t_player.FindChild("Name_Strings").GetComponent<Text>();
        name_text.color = new Color(1, 1, 0, 1);
        name_text.text = s_player[1];
        Text scores_text = t_player.FindChild("Scores_Strings").GetComponent<Text>();
        scores_text.color = new Color(1, 1, 0, 1);
        scores_text.text = s_player[2];


        //crea uno spazio vuoto tra il punteggio del giocatore e la classifica
        Transform t_void = Instantiate(Scores_Layout).transform;
        t_void.SetParent(NextScores_transform, false);
        t_void.FindChild("Rank_Strings").GetComponent<Text>().text = "";
        t_void.FindChild("Name_Strings").GetComponent<Text>().text = "";
        t_void.FindChild("Scores_Strings").GetComponent<Text>().text = "";

        //divide le varie statistiche e le mostra nella classifica

        for (int i = 0; i < rows.Length; i++)
        {
            string[] s = SplitStatistics(rows[i]);

            Transform t = Instantiate(Scores_Layout).transform;
            t.SetParent(NextScores_transform, false);
            t.FindChild("Rank_Strings").GetComponent<Text>().text = s[0];
            t.FindChild("Name_Strings").GetComponent<Text>().text = s[1];
            t.FindChild("Scores_Strings").GetComponent<Text>().text = s[2];
        }
        
        Score_HighScore.gameObject.SetActive(false);
    }

    private string[] SplitStatistics(string row)
    {
        string[] _returnedValue = new string[3];

        int firstIndex = 5; //rank= -> '=' è il char row[4], quindi row[5] sarà l'inizio del rank
        int lengthIndex = 0;

        //========================================= controlla il rank =========================================
        for (int j = firstIndex; j < row.Length; j++)
        {
            if (row[j] == '-')
            {
                lengthIndex = j - firstIndex;
                break;
            }
        }

        _returnedValue[0] = row.Substring(firstIndex, lengthIndex);

        //inizializza per quando bisognerà cercare il nome
        int FirstIndexName = firstIndex + lengthIndex + 6; //-name= -> dopo il trattino ci sono 5 char prima dell'inizio del nome

        //========================================= controlla il punteggio =========================================
        firstIndex += lengthIndex;

        for (int j = row.Length - 1; j > firstIndex; j--)
        {
            if (row[j] == '=')
            {
                //controlla che sia giusto
                string check = row.Substring(j - 7, 8);
                if (check == "-scores=")
                {
                    //j +1 = sarà il char dopo '='
                    lengthIndex = row.Length - (j + 1);
                    _returnedValue[2] = row.Substring(j + 1, lengthIndex);

                    firstIndex = j - 7;//j è '=' e prima ci sono 7 caratteri "-scores"
                    break;
                }
            }
        }

        //========================================= controlla il nome =========================================
        lengthIndex = firstIndex - FirstIndexName;
        _returnedValue[1] = row.Substring(FirstIndexName, lengthIndex);


        return _returnedValue;
    }

}
