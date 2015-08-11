using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class IsoDemo : MonoBehaviour 
{
    List<GameObject> m_listCreatures = new List<GameObject>();
    List<Vector3> m_listVeloc = new List<Vector3>();

    public GameObject PrefabCreature;
    public int NbOfCreatures = 50;
    public float TimeOfCration = 1f;

	// Use this for initialization
	void Start () 
    {
        InvokeRepeating("CreateCreature", TimeOfCration, TimeOfCration);
	}
	
	// Update is called once per frame
	void FixedUpdate () 
    {
	    for( int i = 0; i < m_listCreatures.Count; ++i )
        {
            GameObject obj = m_listCreatures[i];
            Vector3 veloc = m_listVeloc[i];
            obj.transform.position += veloc;
            if (obj.transform.position.x > 200) veloc.x = -veloc.x;
            if (obj.transform.position.x < -200) veloc.x = -veloc.x;
            if (obj.transform.position.y > 100) veloc.y = -veloc.y;
            if (obj.transform.position.y < -100) veloc.y = -veloc.y;
            m_listVeloc[i] = veloc;
        }
	}

    void CreateCreature()
    {
        if (m_listCreatures.Count < NbOfCreatures)
        {
            GameObject obj = (GameObject)Instantiate(PrefabCreature);
            obj.name += m_listCreatures.Count;
            m_listCreatures.Add(obj);
            m_listVeloc.Add((new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized * (Random.value+0.25f) ));
        }
    }
}
