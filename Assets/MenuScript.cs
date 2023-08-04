using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class MenuScript : MonoBehaviour
{
    public RectTransform Menu;
    public RectTransform MenuOpzioni;
    public ControlloGioco control;
    public HighScoreController leaderboard;
    public Text notify;

    public GameObject leaderboard_object;
    public GameObject nome_inputField;
    public GameObject quit_buttons;

    public int ID = 0;
    public string Name = "guest";

    public Text speed_t;
    public int speedIndex, speed_cost;
    public int maxSpeed = 100;
    public Text color_t;
    public int colorIndex, color_cost;

    bool abilitato;

    void Start()
    {
        Menu.anchoredPosition = new Vector2(100, 0);
        MenuOpzioni.anchoredPosition = new Vector2(-100, 0);

        abilitato = true;
        Abilita();
    }

    public void Abilita()
    {
        bool temp_enabled = abilitato;
        abilitato = !temp_enabled;

        StopCoroutine("MoveMenu");

        if (temp_enabled)
        {
            //chiudi menù
            StartCoroutine("MoveMenu", false);
        }
        else
        {
            //apri menù
            StartCoroutine("MoveMenu", true);
        }
    }

    IEnumerator MoveMenu(bool aprire)
    {
        if (aprire)
        {
            Menu.gameObject.SetActive(true);
            MenuOpzioni.gameObject.SetActive(true);

            while (Menu.anchoredPosition.x > 0 && aprire)
            {
                Menu.anchoredPosition -= new Vector2(300 * Time.deltaTime, 0);
                MenuOpzioni.anchoredPosition += new Vector2(300 * Time.deltaTime, 0);
                yield return null;
            }
            
            Menu.anchoredPosition = new Vector2(0, 0);
            MenuOpzioni.anchoredPosition = new Vector2(0, 0);
        }
        else
        {
            leaderboard_object.SetActive(false);
            nome_inputField.SetActive(false);
            quit_buttons.SetActive(false);

            while (Menu.anchoredPosition.x < 100 && !aprire)
            {
                Menu.anchoredPosition += new Vector2(300 * Time.deltaTime, 0);
                MenuOpzioni.anchoredPosition -= new Vector2(300 * Time.deltaTime, 0);
                yield return null;
            }

            Menu.anchoredPosition = new Vector2(100, 0);
            MenuOpzioni.anchoredPosition = new Vector2(-100, 0);

            Menu.gameObject.SetActive(false);
            MenuOpzioni.gameObject.SetActive(false);
        }
    }


    public void AddSpeed(Text costo)
    {
        if (costo != null)
        {
            float f = 2.025f - 0.025f * (speedIndex +1);

            if (f < 0.34f || control.Punteggio < speed_cost)
                return;

            control.AggiungiPunteggio(-speed_cost);

            speed_cost = 10 * speedIndex * speedIndex * 2;

            speedIndex += 1;

            costo.text = "Speed - " + speedIndex + "\n" + speed_cost;

            control.Salvataggio();
        }
        else
        {
            if(speedIndex == 0)
            {
                speed_cost = 10;
                speedIndex += 1;
            }
            GameObject.Find("Speed_t").GetComponent<Text>().text = "Speed - " + speedIndex + "\n" + speed_cost;
        }

        float timeMax = 2.025f - 0.025f * speedIndex;
        timeMax = Mathf.Clamp(timeMax, 0.35f, 2f);

        float timeMin = timeMax - 0.025f * speedIndex;
        timeMin = Mathf.Clamp(timeMin, 0.15f, 2f);

        control.timeMax = timeMax;
        control.timeMin = timeMin;
    }

    public void AddColor(Image bottone)
    {
        if (bottone != null)
        {
            if (colorIndex >= control.colors.Length || control.Punteggio < color_cost)
                return;

            control.AggiungiPunteggio(-color_cost);

            color_cost = 100 * colorIndex * colorIndex * 2;

            colorIndex += 1;

            Color32 c = control.colors[colorIndex - 1];
            bottone.color = new Color32(c.r, c.g, c.b, 255);

            Text t = bottone.GetComponentInChildren<Text>();
            t.text = "Color - " + colorIndex + "\n" + color_cost;

            control.Salvataggio();
        }
        else
        {
            if(colorIndex == 0)
            {
                color_cost = 100;
                colorIndex += 1;
            }
            GameObject go = GameObject.Find("Color_t");
            go.GetComponent<Text>().text = "Color - " + colorIndex + "\n" + color_cost;
            Color32 c = control.colors[colorIndex - 1];
            go.transform.parent.GetComponent<Image>().color = new Color32(c.r, c.g, c.b, 255);
        }

        control.colorLimit = colorIndex;
    }

    public void ShowLeaderBoard()
    {
        bool b = !leaderboard_object.activeSelf;
        leaderboard_object.SetActive(b);
        if (b)
        {
            nome_inputField.SetActive(false);
            quit_buttons.SetActive(false);
            ChangeName(null);

            StartCoroutine(leaderboard.PostScores(ID, Name, control.PunteggioComplessivo, this));
        }
    }

    public void ChangeName(InputField nome)
    {
        //quando si finisce di scrivere nell'inputfield e viene aggiornato il nome
        if (nome != null)
        {
            if (nome.text == "")
                nome.text = "guest";

            Name = nome.text;

            control.Salvataggio();
        }

        GameObject.Find("Name_t").GetComponent<Text>().text = "Name:\n" + Name;
    }

    public void modifyName(InputField nome)
    {
        //quando clicchi sul tasto Name e appare o scompare l'inputField
        bool b = !nome_inputField.activeSelf;
        nome_inputField.SetActive(b);
        if(b)
        {
            leaderboard_object.SetActive(false);
            quit_buttons.SetActive(false);
            nome.text = Name;
        }
    }

    public void LeaveTheGame()
    {
        bool b = !quit_buttons.activeSelf;
        quit_buttons.SetActive(b);
        if(b)
        {
            leaderboard_object.SetActive(false);
            nome_inputField.SetActive(false);
        }
    }

    public void LeaveButtonPressed()
    {
        Application.Quit();
    }

    public void ResumeButtonPressed()
    {
        quit_buttons.SetActive(false);
    }

    public void Refresh(int t_ID, string t_name, int t_speedIndex, int t_speed_cost, int t_colorIndex, int t_color_cost)
    {
        ID = t_ID;
        Name = t_name;
        ChangeName(null);
        speedIndex = t_speedIndex;
        speed_cost = t_speed_cost;
        AddSpeed(null);
        colorIndex = t_colorIndex;
        color_cost = t_color_cost;
        AddColor(null);
    }
}
