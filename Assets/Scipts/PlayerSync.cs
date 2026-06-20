using UnityEngine;
using Photon.Pun;

public class PlayerSync : MonoBehaviourPun, IPunObservable
{
    private float networkAngle;

    void Update()
    {
        if (photonView.IsMine) return;

        transform.rotation = Quaternion.Lerp(transform.rotation,Quaternion.Euler(0, 0, networkAngle),Time.deltaTime * 15f);
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(transform.rotation.eulerAngles.z);
        }
        else
        {
            networkAngle = (float)stream.ReceiveNext();
        }
    }
}