using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#region Comment
/// <summary>
/// <b>유니티에서 제공하는 코루틴 기능의 제어를 보조합니다</b>
/// <para></para>
/// - 하나의 루틴만이 실행되는 것을 보장합니다.
/// <br></br>
/// - 진행중인 루틴이 종료되었는지의 여부를 확인할 수 있습니다.
/// <br></br>
/// <i>! 진행한 루틴이 종료되었을 때에는, 반드시 Finish()함수를 호출해야 합니다.</i>
/// <br></br>
/// </summary>
#endregion
public class Coroutine
{
    private MonoBehaviour _User;
    private IEnumerator _CurrentRoutine;

    public bool IsOver => _CurrentRoutine == null;

    public Coroutine(MonoBehaviour user)
    {
        _User = user;
    }
    public void Start(IEnumerator routine)
    {
        Stop();
        _User.StartCoroutine(_CurrentRoutine = routine);
    }
    public void Stop()
    {
        if (_CurrentRoutine != null)
        {
            _User.StopCoroutine(_CurrentRoutine);
            _CurrentRoutine = null;
        }
    }
    public void Finish()
    {
        _CurrentRoutine = null;
    }
}
