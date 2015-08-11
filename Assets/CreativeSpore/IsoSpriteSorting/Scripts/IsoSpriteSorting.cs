
/*
 * Enabling this define call UpdateSorting from OnWillRenderObject event, instead finding if there is any render part visible for each IsoSpriteSorting object.
 * If this is enabled, the object containing IsoSpriteSorting script should have a render so OnWillRenderObject is called.
 * I have seen no performance improvement disabling this define, so I have left this commented to make it optional placing the IsoSpriteSorting script in an object with a renderer.
 */
//#define UPDATE_SORTING_ON_WILL_RENDER_OBJECT

using UnityEngine;
using System.Collections;
using System.Collections.Generic;


namespace CreativeSpore.SpriteSorting
{

    [ExecuteInEditMode]
    public class IsoSpriteSorting : MonoBehaviour
    {
        //NOTE: set to -32768 instead if you are displaying more than 32767 sprites to allow sorting properly another 32768 extra sprites.
        // Or set to 0 to see the order number better (starting with 0) as so many sprites at the same time would be crazy
        public const int k_BaseSortingValue = -32768;
        public const string k_IntParam_ExecuteInEditMode = "IsoSpriteSorting_ExecuteInEditMode";

        /// <summary>
        /// Data for each separated axis
        /// </summary>
        class AxisData
        {
            public List<IsoSpriteSorting> ListSortedIsoSprs = new List<IsoSpriteSorting>();
            public int OrderCnt = k_BaseSortingValue;
            public int LastFrameCnt = 0;
        };

        /// <summary>
        /// Sorting axis
        /// </summary>
        public enum eSortingAxis
        {
            X,
            Y,
            Z
        }

        static Dictionary<IsoSpriteSorting, Renderer[]> s_dicInstanceSpriteList = new Dictionary<IsoSpriteSorting, Renderer[]>();


        /// <summary>
        /// Separation of management by sorting axis
        /// </summary>
        static AxisData[] s_axisData = new AxisData[]
        {
            new AxisData(), //x
            new AxisData(), //y
            new AxisData(), //z
        };

        /// <summary>
        /// Sorting axis
        /// </summary>
        public eSortingAxis SorterAxis = eSortingAxis.Y;
        public Vector3 SorterPositionOffset = new Vector3();

        /// <summary>
        /// If invalidated and IncludeInactiveRenderer is true, inactive renderers will be taking into account
        /// </summary>
        public bool IncludeInactiveRenderer = true;

        private bool m_invalidated = true;

        /// <summary>
        /// Invalidate when adding or removing any renderer
        /// </summary>
        public void Invalidate()
        {
            m_invalidated = true;
        }

        /// <summary>
        /// Invalidate all objects
        /// </summary>
        public void InvalidateAll()
        {
            foreach( IsoSpriteSorting obj in s_dicInstanceSpriteList.Keys )
            {
                obj.Invalidate();
            }
        }

        public string GetStatistics()
        {
            string sStats = "";
            
            int nbOfVisibleObjs = 0;
            int nbOfRenderers = 0;

            sStats = "<b>Stats By Axis:</b>\n";
            for (int i = 0; i < 3; ++i )
            {
                sStats += "  <b>- "+(eSortingAxis)i+":</b>\n";
                if( s_axisData[i].ListSortedIsoSprs.Count > 0 )
                {
                    int orderCntRelToBase = s_axisData[i].OrderCnt - k_BaseSortingValue;
                    nbOfVisibleObjs += s_axisData[i].ListSortedIsoSprs.Count;
                    nbOfRenderers += orderCntRelToBase;
                    sStats += "    <b>Total Visible Objects: </b>" + s_axisData[i].ListSortedIsoSprs.Count + "\n";
                    sStats += "    <b>Total Visible Renderers: </b>" + orderCntRelToBase + "\n";
                }
            }

            sStats += "\n<b>Global Stats:</b>\n";
            sStats += "  <b>Total Registered Objects: </b>" + s_dicInstanceSpriteList.Keys.Count + "\n";
            sStats += "  <b>Total Visible Objects: </b>" + nbOfVisibleObjs + "\n";
            sStats += "  <b>Total Visible Renderers: </b>" + nbOfRenderers + "\n";

            return sStats;
        }

        void Start()
        {
            Invalidate();
        }

        void Update()
        {

            if (m_invalidated)
            {
                m_invalidated = false;
                Renderer[] outList;
                outList = GetComponentsInChildren<Renderer>( IncludeInactiveRenderer );
                System.Array.Sort(outList, (a, b) => a.sortingOrder.CompareTo(b.sortingOrder));
                s_dicInstanceSpriteList[this] = outList;
            }

            bool isVisible = false;
#if !UPDATE_SORTING_ON_WILL_RENDER_OBJECT
            Renderer[] aSprRenderer = null;
            s_dicInstanceSpriteList.TryGetValue(this, out aSprRenderer);
            if (aSprRenderer != null)
            {
                for (int i = 0; i < aSprRenderer.Length && !isVisible; ++i)
                {
                    isVisible = aSprRenderer[i].isVisible;
                }
            }
#endif

            if ( isVisible || !Application.isPlaying && PlayerPrefs.GetInt(IsoSpriteSorting.k_IntParam_ExecuteInEditMode, 1) != 0)
            {
                UpdateSorting();
            }
        }

        void UpdateSorting()
        {            
            int iSortingAxis = (int)SorterAxis;
            List<IsoSpriteSorting> listSortedIsoSpr = s_axisData[iSortingAxis].ListSortedIsoSprs;
            if (Time.frameCount != s_axisData[iSortingAxis].LastFrameCnt)
            {
                s_axisData[iSortingAxis].LastFrameCnt = Time.frameCount;
                s_axisData[iSortingAxis].OrderCnt = k_BaseSortingValue; 

                //Sort sprites
                switch (SorterAxis)
                {
                    case eSortingAxis.X: listSortedIsoSpr.Sort((a, b) => (b.SorterPositionOffset.x + b.transform.position.x).CompareTo(a.SorterPositionOffset.x + a.transform.position.x)); break;
                    case eSortingAxis.Y: listSortedIsoSpr.Sort((a, b) => (b.SorterPositionOffset.y + b.transform.position.y).CompareTo(a.SorterPositionOffset.y + a.transform.position.y)); break;
                    case eSortingAxis.Z: listSortedIsoSpr.Sort((a, b) => (b.SorterPositionOffset.z + b.transform.position.z).CompareTo(a.SorterPositionOffset.z + a.transform.position.z)); break;
                }
                for (int i = 0; i < listSortedIsoSpr.Count; ++i)
                {
                    Renderer[] aSprRenderer = null;
                    listSortedIsoSpr[i].m_invalidated = !s_dicInstanceSpriteList.TryGetValue(listSortedIsoSpr[i], out aSprRenderer);
                    if (aSprRenderer != null)
                    {
                        for (int j = 0; j < aSprRenderer.Length; ++j)
                        {
                            if (aSprRenderer[j] != null)
                                aSprRenderer[j].sortingOrder = s_axisData[iSortingAxis].OrderCnt++;
                            else // a renderer was destroyed
                                listSortedIsoSpr[i].m_invalidated = true;
                        }
                    }
                }
                listSortedIsoSpr.Clear();
            }
            
            listSortedIsoSpr.Add(this);
        }

#if UPDATE_SORTING_ON_WILL_RENDER_OBJECT
        void OnWillRenderObject()
        {
            UpdateSorting();
        }
#endif

        void OnDestroy()
        {
            s_dicInstanceSpriteList.Remove(this);
            s_axisData[(int)SorterAxis].ListSortedIsoSprs.Remove(this);
        }
    }
}