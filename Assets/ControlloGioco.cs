using UnityEngine;
using UnityEngine.UI;
using System.Collections;
#if UNITY_ADS
using UnityEngine.Advertisements;
#endif

public class ControlloGioco : MonoBehaviour
{
    public GameObject button;

    public Text Score;
    public Text dataPath;
    public Text notify;

    public Color32[] colors;
    public int colorLimit = 1;
    
    public int Punteggio = 0;
    public int PunteggioComplessivo = 0;

    MenuScript menu;
    private Camera cam;

    private float mezzoBottone, screenWidth;
    private bool lastAds;

    public float timeMin, timeMax = 2;

    void Start()
    {
        cam = Camera.main;
        menu = FindObjectOfType<MenuScript>();
        SalvaGioco.LoadData(this, menu);

        lastAds = false;

        AggiungiPunteggio(0);

        mezzoBottone = ButtonToScreen(button.GetComponent<Renderer>().bounds);

        dataPath.text = "ID: " + menu.ID + "\nData:\n" + Application.persistentDataPath;

        notify.CrossFadeAlpha(0.0f, 0.0f, true);

        //InvokeRepeating("ControlloCambioOrientazione", 0, 2);

        StartCoroutine("AfterSplashScreen");
    }

    IEnumerator AfterSplashScreen()
    {
        while (Application.isShowingSplashScreen == true)
            yield return null;
        
        InvokeRepeating("SalvataggioAutomatico", 300, 300);

        InstanziaBottone();

        yield return new WaitForSeconds(1f);

        notify.text = "Press 'Back Button' to open the menu";
        notify.CrossFadeAlpha(1.0f, 0.5f, false);

        Invoke("CrossFadeOut", 3f);
    }

    void InstanziaBottone()
    {
        ControlloCambioOrientazione();

        if (Random.Range(1, 10001) > 1)
        {
            //spawna un bottone di dimensioni 1x1, in una parte random dello schermo

            //x = random dentro alla screen.width - y = sopra lo screen.height - z = distanza massima della camera /2
            Vector3 screenPosition = cam.ScreenToWorldPoint(new Vector3(Random.Range(mezzoBottone, Screen.width - mezzoBottone),
                Screen.height + mezzoBottone * 2,
                cam.farClipPlane / 2));

            GameObject go = (GameObject)Instantiate(button, screenPosition, Quaternion.identity);

            ButtonScript bs = go.GetComponent<ButtonScript>();
            //con gli int, il max è escluso -> quindi va da 1 alla lunghezza
            int life = Random.Range(1, colorLimit + 1);
            bs.life = life;
            CambiaColoreQuadrati(go, life);

            float speed = colorLimit + 1 - life;
            speed = 1 + (speed * 1.5f / 10);
            bs.speed = speed;
        }
        else
        {
            //spawna bottone nero grande quanto lo schermo con vita 100

            Vector3 posStart = cam.ScreenToWorldPoint(new Vector3(0, 0));
            Vector3 posEnd = cam.ScreenToWorldPoint(new Vector3(Screen.width - (mezzoBottone *2), Screen.height));

            float widthX = (posEnd.x - posStart.x);
            float widthBottone = BigButtonToScreen(widthX);

            Vector3 screenPosition = cam.ScreenToWorldPoint(new Vector3(Screen.width /2, Screen.height + widthBottone /2, cam.farClipPlane / 2));
            GameObject go = (GameObject)Instantiate(button, screenPosition, Quaternion.identity);

            go.transform.localScale = new Vector3(widthX, widthX, widthX);
            go.GetComponent<BoxCollider2D>().size = new Vector2(1, 1);

            ButtonScript bs = go.GetComponent<ButtonScript>();
            int life = 100;
            bs.life = life;
            CambiaColoreQuadrati(go, life);

            bs.speed = 0.5f;
        }

        Invoke("InstanziaBottone", Random.Range(timeMin, timeMax));
    }

    public void AggiungiPunteggio(int p)
    {
        Punteggio += p;
        if(p > 0)
            PunteggioComplessivo += p;

        Score.text = Punteggio.ToString();
    }

    float ButtonToScreen(Bounds bounds)
    {
        Vector3 posStart = cam.WorldToScreenPoint(new Vector3(bounds.min.x, bounds.min.y, bounds.min.z));
        Vector3 posEnd = cam.WorldToScreenPoint(new Vector3(bounds.max.x, bounds.max.y, bounds.min.z));

        float widthX = (posEnd.x - posStart.x);
        //int widthY = (int)(posEnd.y - posStart.y);

        float halfWidth = widthX /1.5f;

        return halfWidth;
    }

    float BigButtonToScreen(float f)
    {
        Vector3 posStart = cam.WorldToScreenPoint(new Vector3(0, 0));
        Vector3 posEnd = cam.WorldToScreenPoint(new Vector3(f, f));

        float widthX = (posEnd.x - posStart.x);

        return widthX;
    }

    void ControlloCambioOrientazione()
    {
        if(screenWidth != Screen.width)
        {
            screenWidth = Screen.width;
            mezzoBottone = ButtonToScreen(button.GetComponent<Renderer>().bounds);
        }
    }

    public void CambiaColoreQuadrati(GameObject b, int l)
    {
        if (l > colors.Length)
        {
            Material m = b.GetComponent<Renderer>().material;
            m.color = Color.black;
        }
        else
        {
            Material m = b.GetComponent<Renderer>().material;
            m.color = colors[l - 1];
        }
    }


    void OnApplicationQuit()
    {
        SalvaGioco.SaveData(this, menu);
    }

    void OnApplicationPause(bool pausa)
    {
        if (pausa)
        {
            SalvaGioco.SaveData(this, menu);
        }
    }

    public void Salvataggio()
    {
        SalvaGioco.SaveData(this, menu);

        notify.text = "Saved";
        notify.CrossFadeAlpha(1.0f, 0.5f, false);

        Invoke("CrossFadeOut", 3f);
    }

    private void SalvataggioAutomatico()
    {
        SalvaGioco.SaveData(this, menu);

        bool showAds = false;
#if UNITY_ADS
        bool showDefaultVideo = true;
        if (!Advertisement.isShowing)
        {
            if (Advertisement.IsReady("video"))
            {
                showAds = true;
            }
            else if (Advertisement.IsReady("rewardedVideo"))
            {
                showAds = true;
                showDefaultVideo = false;
            }
        }
#endif

        if (showAds && lastAds == false)
        {
#if UNITY_ADS
            ShowOptions adsOptions = new ShowOptions();
            adsOptions.resultCallback = HandleShowResult;

            if (showDefaultVideo)
                Advertisement.Show("video", adsOptions);
            else
                Advertisement.Show("rewardedVideo", adsOptions);
#endif
        }
        else
        {
#if UNITY_ADS
            notify.text = "Saved";

            if(showAds == false && lastAds == false)
                notify.text = "Saved - Advertisment is not ready";

            lastAds = false;
#else
            notify.text = "Saved";
#endif

            //  fade from transparent over 500ms.
            notify.CrossFadeAlpha(1.0f, 0.5f, false);

            Invoke("CrossFadeOut", 3f);
        }
    }

    private void CrossFadeOut()
    {
        // fade to transparent over 500ms.
        notify.CrossFadeAlpha(0.0f, 0.5f, false);
    }

#if UNITY_ADS
    private void HandleShowResult(ShowResult result)
    {
        int temp_punteggio = 0;
        switch (result)
        {
            case ShowResult.Finished:
                temp_punteggio = 50 * colorLimit * colorLimit * 2;
                notify.text = "Video completed. User rewarded " + (temp_punteggio) + " credits.";
                break;
            case ShowResult.Skipped:
                temp_punteggio = 5 * colorLimit * colorLimit * 2;
                notify.text = "Video was skipped. User rewarded only " + (temp_punteggio) + " credits";
                break;
            case ShowResult.Failed:
                notify.text = "Video failed to show.";
                break;
        }

        if(temp_punteggio > 0)
        {
            lastAds = true;
            AggiungiPunteggio(temp_punteggio);
            SalvaGioco.SaveData(this, menu);
        }

        //  fade from transparent over 500ms.
        notify.CrossFadeAlpha(1.0f, 0.5f, false);

        Invoke("CrossFadeOut", 3f);
    }
#endif

}
