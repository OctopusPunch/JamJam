using UnityEngine;

public class MonsterStateMachine : MonoBehaviour
{
    private StateMachine stateMachine;

    private void Awake()
    {
        stateMachine = new StateMachine();

        stateMachine.ChangeState(new IdleState());
    }

}
