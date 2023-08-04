using UnityEngine;

public class PlayerInput : MonoBehaviour
{
    public ControlloGioco control;

    void Update ()
    {
        bool clicked = false;

        if(Input.GetMouseButtonDown(0))
        {
            clicked = true;

            control.AggiungiPunteggio(1);

            RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);

            if(hit.collider != null)
            {
                Danneggia(hit);
            }
        }

        if(Input.touchCount > 0)
        {
            int temp_i = clicked ? 1 : 0;
            for (int i = temp_i; i < Input.touchCount; i++)
            {
                //Store the first touch detected.
                Touch myTouch = Input.touches[i];

                int n_touch = 0;

                //Check if the phase of that touch equals Began
                if (myTouch.phase == TouchPhase.Began)
                {
                    n_touch += 1;

                    RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(myTouch.position), Vector2.zero);

                    if (hit.collider != null)
                    {
                        Danneggia(hit);
                    }
                }

                control.AggiungiPunteggio(n_touch);
            }
        }

        if(Input.GetKeyDown(KeyCode.Escape))
        {
            //aprire o chiudere il menù
            FindObjectOfType<MenuScript>().Abilita();
        }
	}

    void Danneggia(RaycastHit2D h)
    {
        Transform t = h.transform;

        if (t.CompareTag("GameController"))
        {
            //colpisci i quadrati
            t.GetComponent<ButtonScript>().TakeDamage();
            GetComponent<AudioSource>().pitch = Random.Range(0.9f, 1.3f);
            GetComponent<AudioSource>().Play();
        }
    }
}
