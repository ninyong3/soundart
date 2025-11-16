using UnityEngine;
/// <summary>
/// 게임이 시작될 때 충돌 영역 면 시각화
/// </summary>
[RequireComponent(typeof(PolygonCollider2D))]
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class DrawingZoneVisualizer : MonoBehaviour
{
    private void Awake()
    {
        PolygonCollider2D polyCollider=GetComponent<PolygonCollider2D>();
        MeshFilter meshFilter = GetComponent<MeshFilter>();
        Vector2[] points=polyCollider.points; // 콜라이더의 모든 꼭짓점 가져오기
        Vector3[] vertices = new Vector3[points.Length];// vector2를 vector3로 변환
        for(int i = 0; i < points.Length; i++) 
        {
            vertices[i] = new Vector3(points[i].x, points[i].y, 0);
        }
        Triangulator tr = new Triangulator(points); // 콜라이더의 점들을 삼각형으로 쪼개기
        int[] triangles = tr.Triangulate();
        Mesh mesh = new Mesh(); // 메시 만들기, 계산된 점, 삼각형 할당
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        Vector2[] uv=new Vector2[points.Length];
        for (int i = 0; i < points.Length; i++)
        {
            uv[i] = points[i];
        }
        mesh.uv = uv;
        meshFilter.mesh=mesh;
    }
}
public class Triangulator
{
    private Vector2[] m_points;
    public Triangulator(Vector2[] points)
    {
        m_points = points;
    }
    public int[] Triangulate()
    {
        System.Collections.Generic.List<int> indices =new System.Collections.Generic.List<int>();
        int n=m_points.Length;
        if(n<3)
            return indices.ToArray();
        int[] V = new int[n];
        if (Area() > 0)
        {
            for (int v = 0; v < n; v++)
            {
                V[v] = v;
            }
        }
        else
        {
            for(int v = 0;v < n; v++)
            {
                V[v] = (n - 1) - v;
            }
        }
        int nv = n;
        int count = 2 * nv;
        for (int m = 0, v = nv - 1; nv > 2;)
        {
            if((count--) <= 0)
                return indices.ToArray();
            int u = v;
            if (nv <= u)
                u = 0;
            v = u + 1;
            if (nv <= v)
                v = 0;
            int w = v + 1;
            if (nv <= w)
                w = 0;
            if(Snip(u, v, w, nv, V))
            {
                int a, b, c, s, t;
                a = V[u];
                b = V[v];
                c = V[w];
                indices.Add(a);
                indices.Add(b);
                indices.Add(c);
                m++;
                for (s = v, t = v + 1; t < nv; s++, t++)
                    V[s] = V[t];
                nv--;
                count = 2 * nv;
            }
        }
        indices.Reverse();
        return indices.ToArray();
    }
    private float Area()
    {
        int n=m_points.Length;
        float A = 0.0f;
        for(int p=n-1, q=0;q<n;p=q++)
        {
            Vector2 pval=m_points[p];
            Vector2 qval=m_points[q];
            A += pval.x * qval.y - qval.x * pval.y;
        }
        return (A * 0.5f);
    }
    private bool Snip(int u, int v, int w, int n, int[] V)
    {
        int p;
        Vector2 A = m_points[V[u]];
        Vector2 B = m_points[V[v]];
        Vector2 C = m_points[V[w]];
        if(Mathf.Epsilon > (((B.x-A.x)*(C.y-A.y))-((B.y-A.y)*(C.x-A.x))))
            return false;
        for(p=0;p<n;p++)
        {
            if ((p == u) || (p == v) || (p == w))
                continue;
            Vector2 P = m_points[V[p]];
            if (InsideTriangle(A, B, C, P))
                return false;
        }
        return true;
    }
    private bool InsideTriangle(Vector2 A, Vector2 B, Vector2 C, Vector2 P)
    {
        float ax, ay, bx, by, cx, cy, apx, apy, bpx, bpy, cpx, cpy;
        float cCROSSap, bCROSScp, aCROSSbp;

        ax = C.x - B.x; ay = C.y - B.y;
        bx = A.x - C.x; by = A.y - C.y;
        cx = B.x - A.x; cy = B.y - A.y;
        apx = P.x - A.x; apy = P.y - A.y;
        bpx = P.x - B.x; bpy = P.y - B.y;
        cpx = P.x - C.x; cpy = P.y - C.y;

        aCROSSbp = ax * bpy - ay * bpx;
        cCROSSap = cx * apy - cy * apx;
        bCROSScp = bx * cpy - by * cpx;

        return ((aCROSSbp >= 0.0f) && (bCROSScp >= 0.0f) && (cCROSSap >= 0.0f));
    }
}