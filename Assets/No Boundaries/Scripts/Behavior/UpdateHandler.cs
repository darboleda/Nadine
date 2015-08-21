using UnityEngine;
using System.Collections;

public abstract class UpdateHandler : MonoBehaviour
{
    public abstract void Handle(UpdateInfo update);
}
