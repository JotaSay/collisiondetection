using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PrismManager : MonoBehaviour
{
    public int prismCount = 10;
    public float prismRegionRadiusXZ = 5;
    public float prismRegionRadiusY = 5;
    public float maxPrismScaleXZ = 5;
    public float maxPrismScaleY = 5;
    public GameObject regularPrismPrefab;
    public GameObject irregularPrismPrefab;

    private List<Prism> prisms = new List<Prism>();
    private List<GameObject> prismObjects = new List<GameObject>();
    private GameObject prismParent;
    private Dictionary<Prism,bool> prismColliding = new Dictionary<Prism, bool>();

    private const float UPDATE_RATE = 0.5f;

    #region Unity Functions

    void Start()
    {
        Random.InitState(0);    //10 for no collision

        prismParent = GameObject.Find("Prisms");
        for (int i = 0; i < prismCount; i++)
        {
            var randPointCount = Mathf.RoundToInt(3 + Random.value * 7);
            var randYRot = Random.value * 360;
            var randScale = new Vector3((Random.value - 0.5f) * 2 * maxPrismScaleXZ, (Random.value - 0.5f) * 2 * maxPrismScaleY, (Random.value - 0.5f) * 2 * maxPrismScaleXZ);
            var randPos = new Vector3((Random.value - 0.5f) * 2 * prismRegionRadiusXZ, (Random.value - 0.5f) * 2 * prismRegionRadiusY, (Random.value - 0.5f) * 2 * prismRegionRadiusXZ);

            GameObject prism = null;
            Prism prismScript = null;
            if (Random.value < 0.5f)
            {
                prism = Instantiate(regularPrismPrefab, randPos, Quaternion.Euler(0, randYRot, 0));
                prismScript = prism.GetComponent<RegularPrism>();
            }
            else
            {
                prism = Instantiate(irregularPrismPrefab, randPos, Quaternion.Euler(0, randYRot, 0));
                prismScript = prism.GetComponent<IrregularPrism>();
            }
            prism.name = "Prism " + i;
            prism.transform.localScale = randScale;
            prism.transform.parent = prismParent.transform;
            prismScript.pointCount = randPointCount;
            prismScript.prismObject = prism;

            prisms.Add(prismScript);
            prismObjects.Add(prism);
            prismColliding.Add(prismScript, false);
        }

        StartCoroutine(Run());
    }
    
    void Update()
    {
        #region Visualization

        DrawPrismRegion();
        DrawPrismWireFrames();

#if UNITY_EDITOR
        if (Application.isFocused)
        {
            UnityEditor.SceneView.FocusWindowIfItsOpen(typeof(UnityEditor.SceneView));
        }
#endif

        #endregion
    }

    IEnumerator Run()
    {
        yield return null;

        while (true)
        {
            foreach (var prism in prisms)
            {
                prismColliding[prism] = false;
            }

            foreach (var collision in PotentialCollisions())
            {
                if (CheckCollision(collision))
                {
                    prismColliding[collision.a] = true;
                    prismColliding[collision.b] = true;

                    ResolveCollision(collision);
                }
            }

            yield return new WaitForSeconds(UPDATE_RATE);
        }
    }

    #endregion

    #region Incomplete Functions

    private IEnumerable<PrismCollision> PotentialCollisions()
    {
        List < belonger > meep = new List<belonger>();
        for (int i = 0; i < prisms.Count; i++)
        {
            belonger minx = new belonger();
            belonger maxx = new belonger();
            minx.xval = prisms[i].points.Min(point => point.x);
            maxx.xval = prisms[i].points.Max(point => point.x);
            maxx.max = true;
            minx.max = false;
            Debug.Log("these are our generated x vals" + minx.xval + ' ' + maxx.xval);
            minx.p = maxx.p = prisms[i];
            meep.Add(minx);
            meep.Add(maxx);
        }
        foreach (belonger b in meep)
        {
            Debug.Log("this is supposed to be the list" + b.xval + "this is if its max" + b.max);

        }

        meep.Sort(delegate (belonger x, belonger y) {return x.xval.CompareTo(y.xval);});
        foreach (belonger b in meep)
        {
            Debug.Log("this is supposed to be the sorted list entry" + b.xval + "this is if its max" + b.max);
        }

        Debug.Log("This is supposed to be the ordered list" + meep);
        List < Prism > activelist = new List<Prism>();
        foreach (belonger b in meep)
        {       
        if (b.max == false)
                    {
        foreach (Prism p in activelist)
                {
                    var checkPrisms = new PrismCollision();
                    checkPrisms.a = p;
                    checkPrisms.b = b.p;
                    yield return checkPrisms;

                }

                activelist.Add(b.p);


            }
            else
            {
                activelist.Remove(b.p);
            }
        }

        yield break;
    }
    private class belonger
    {
        public float xval;
        public bool max;
        public Prism p;
    }

    private bool CheckCollision(PrismCollision collision)
    {
        var prismA = collision.a;
        var prismB = collision.b;
        Vector3 a, b, c, ab, ac, ao, abPerp, acPerp;
        bool ans = false;
        List<Vector3> simplex = new List<Vector3>();
        Vector3 direction = new Vector3(1,1,1);
        simplex.Add(getSupportVector(collision, direction));
        direction = -direction;
        while(true)
        {  
            simplex.Add(getSupportVector(collision , direction));
            if (Vector3.Dot(simplex.Last(), direction) < 0)
            {
                //print(Vector3.Dot(simplex.Last(), direction));
                ans = false;
                break;
            }               
            else
            {
                a = simplex.Last();
                ao = -a;
                if (simplex.Count == 3) 
                {
                    b = simplex[1];
                    c = simplex[0];
                    ab = b - a;
                    ac = c - a;
                    //AxBxC = B(C.dot(A)) – A(C.dot(B))
                    //tripleProduct(ac, ab, ab);
                    //tripleProduct(ab, ac, ac)
                    abPerp = ab * Vector3.Dot(ab, ac) - ac * Vector3.Dot(ab, ab);
                    acPerp = ac * Vector3.Dot(ac, ab) - ab * Vector3.Dot(ac, ac);
                    if (Vector3.Dot(abPerp, ao) > 0) 
                    {
                        simplex.Remove(c);
                        direction = abPerp;
                    } 
                    else 
                    {
                        if (Vector3.Dot(acPerp, ao) > 0) {
                            simplex.Remove(b);
                            direction = acPerp;
                        } else{
                            ans = true;
                            break;
                        }
                    }
                } 
                else 
                {
                    b = simplex[1];
                    ab = b - a;
                    //abPerp = tripleProduct(ab, ao, ab);
                    abPerp = ao * Vector3.Dot(ab, ab) - ab * Vector3.Dot(ab, ao);
                    direction = abPerp;
                }
            }
        }

        Vector3 normal = Vector3.zero;
        float depth = 0f;
        float closestDist;
        Vector3 closestNormal;
        int closestIndex;
        while (true)
        {
            closestDist = 1000000000000f;
            closestNormal = Vector3.zero;
            closestIndex = -1;

            for (int i = 0; i < simplex.Count; i++)
            {      
                int j = i + 1 == simplex.Count ? 0 : i + 1;
                a = simplex[i];
                b = simplex[j];
                c = b - a;
                ab = a * Vector3.Dot(c, c) - c * Vector3.Dot(c, a);
                /*print(a);
                print(b);
                print(c);
                print(ab);*/
                Vector3.Normalize(ab);
                float d = Vector3.Dot(ab, a);
                if (d < closestDist)
                {
                    closestDist = d;
                    closestNormal = ab;
                    closestIndex = j;
                }
            }
            //print(simplex.Count);
            Vector3 p = getSupportVector(collision, closestNormal);
            float ds = Vector3.Dot(p, closestNormal);
            if (ds - closestDist < 0.5)
            {
                normal = closestNormal;
                depth = ds;
                break;
            }
            else
            {
                simplex.Insert(closestIndex, p);
            }
        }
        print(depth);
        collision.penetrationDepthVectorAB = normal * 2*(1/1-depth);
        return ans;
    }

    private Vector3 getSupportPoint(Vector3[] vertices, Vector3 d)
    {
        float highest = -9999999999999999999f;
        Vector3 support = new Vector3(0,0,0);
        //Vector3 support = new Vector3(0f, 0f, 0f);
        for (int i=0; i< vertices.Length; i++)
        {
            Vector3 v = vertices[i];
            float dotProduct = Vector3.Dot(v, d);
            //print(highest);
            if(highest < dotProduct)
            {
                highest = dotProduct;
                support = v;
            }
        }
        return support;
    }

    private Vector3 getSupportVector(PrismCollision collision, Vector3 d)
    {
        var prismA = collision.a;
        var prismB = collision.b;
        Vector3 aPoint = getSupportPoint(prismA.points, d);
        Vector3 bPoint = getSupportPoint(prismB.points, -d);
        //print(aPoint);
        //print(bPoint);
        //return the minkowski point
        return aPoint - bPoint;
    }
    
    #endregion

    #region Private Functions
    private void ResolveCollision(PrismCollision collision)
    {
        var prismObjA = collision.a.prismObject;
        var prismObjB = collision.b.prismObject;

        var pushA = -collision.penetrationDepthVectorAB;
        var pushB = collision.penetrationDepthVectorAB;

        for (int i = 0; i < collision.a.pointCount; i++)
        {
            collision.a.points[i] += pushA;
        }
        for (int i = 0; i < collision.b.pointCount; i++)
        {
            collision.b.points[i] += pushB;
        }
        //prismObjA.transform.position += pushA;
        //prismObjB.transform.position += pushB;

        Debug.DrawLine(prismObjA.transform.position, prismObjA.transform.position + collision.penetrationDepthVectorAB, Color.cyan, UPDATE_RATE);
    }


    #endregion

    #region Visualization Functions

    private void DrawPrismRegion()
    {
        var points = new Vector3[] { new Vector3(1, 0, 1), new Vector3(1, 0, -1), new Vector3(-1, 0, -1), new Vector3(-1, 0, 1) }.Select(p => p * prismRegionRadiusXZ).ToArray();
        
        var yMin = -prismRegionRadiusY;
        var yMax = prismRegionRadiusY;

        var wireFrameColor = Color.yellow;

        foreach (var point in points)
        {
            Debug.DrawLine(point + Vector3.up * yMin, point + Vector3.up * yMax, wireFrameColor);
        }

        for (int i = 0; i < points.Length; i++)
        {
            Debug.DrawLine(points[i] + Vector3.up * yMin, points[(i + 1) % points.Length] + Vector3.up * yMin, wireFrameColor);
            Debug.DrawLine(points[i] + Vector3.up * yMax, points[(i + 1) % points.Length] + Vector3.up * yMax, wireFrameColor);
        }
    }

    private void DrawPrismWireFrames()
    {
        for (int prismIndex = 0; prismIndex < prisms.Count; prismIndex++)
        {
            var prism = prisms[prismIndex];
            var prismTransform = prismObjects[prismIndex].transform;

            var yMin = prism.midY - prism.height / 2 * prismTransform.localScale.y;
            var yMax = prism.midY + prism.height / 2 * prismTransform.localScale.y;

            var wireFrameColor = prismColliding[prisms[prismIndex]] ? Color.red : Color.green;

            foreach (var point in prism.points)
            {
                Debug.DrawLine(point + Vector3.up * yMin, point + Vector3.up * yMax, wireFrameColor);
            }

            for (int i = 0; i < prism.pointCount; i++)
            {
                Debug.DrawLine(prism.points[i] + Vector3.up * yMin, prism.points[(i + 1) % prism.pointCount] + Vector3.up * yMin, wireFrameColor);
                Debug.DrawLine(prism.points[i] + Vector3.up * yMax, prism.points[(i + 1) % prism.pointCount] + Vector3.up * yMax, wireFrameColor);
            }
        }
    }

    #endregion

    #region Utility Classes

    private class PrismCollision
    {
        public Prism a;
        public Prism b;
        public Vector3 penetrationDepthVectorAB;
    }

    private class Tuple<K,V>
    {
        public K Item1;
        public V Item2;

        public Tuple(K k, V v) {
            Item1 = k;
            Item2 = v;
        }
    }

    #endregion
}
