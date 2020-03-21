using UnityEngine;

public class TouchController : MonoBehaviour {

    private Vector2 startTouchPosition, endTouchPosition;
    private GameBoard gb;

    private void Start()
    {
        gb = GetComponent<GameBoard>();
    }

    private void Update()
    {
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began) {
            startTouchPosition = Input.GetTouch(0).position;
        }
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Ended)
        {
            endTouchPosition = Input.GetTouch(0).position;
            if(Vector2.Distance(startTouchPosition, endTouchPosition) == 0)
            {
                gb.SelectTrioAround(Camera.main.ScreenToWorldPoint(endTouchPosition));
            }
            else if (endTouchPosition.x > startTouchPosition.x)
            {
                gb.TurnCW();
            }
            else
            {
                gb.TurnCCW();
            }
        }
    }
}
