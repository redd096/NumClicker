using UnityEngine;

public class ButtonScript : MonoBehaviour
{
    ControlloGioco control;

    public int life = 0;

    public float speed = 1;


    void Start()
    {
        control = FindObjectOfType<ControlloGioco>();
    }

    void Update()
    {
        transform.Translate(Vector3.down * speed * Time.deltaTime, Space.World);
    }

    public void TakeDamage()
    {
        control.AggiungiPunteggio(life);
        life -= 1;

        if (life <= 0)
            Destroy(gameObject);
        else
            control.CambiaColoreQuadrati(gameObject, life);
    }
}
