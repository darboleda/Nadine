using UnityEngine;
using System;
using System.Collections.Generic;

public class UpdateMap : ScriptableObject {

    public UpdateInfo[] Updates;

    public UpdateInfo GetMappedUpdate(string id)
    {
        foreach (UpdateInfo update in Updates)
        {
            if (update.Id == id) return update;
        }
        return null;
    }
}
