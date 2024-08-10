using UnityEngine;
using System.Collections;

public class PlayerShadowHelper : MonoBehaviour
{

	public CapsuleCollider c;
    public SphereCollider sc;
    Transform player;
    
    MeshRenderer mr;
    // Use this for initialization
    Material mat;
    public bool ShadowIsNotPlayerButParent = false;
	void Start () {
        mr = GetComponent<MeshRenderer>();
        if (ShadowIsNotPlayerButParent) {
            player = transform.parent;
            transform.parent = null;
        } else {
            transform.parent = null;
            player = GameObject.Find("MediumPlayer").transform;
        }

        if (player.GetComponent<CapsuleCollider>() != null) c = player.GetComponent<CapsuleCollider>();
        if (player.GetComponent<SphereCollider>() != null) sc = player.GetComponent<SphereCollider>();

        mat = mr.material;
	}
    Color col = new Color();

    public float maxShadowDistance = 8f;
    public float alphaAtMax = 0.65f;
    public float alphaAtMin = 0.8f;
    public float minShadowScale = 2f;
    public float maxShadowScale = 4f;
    public float floorOffset = 0.4f;
    public float fadeInTime = 0.2f;
    float t_fadeInTime = 0;

    Vector3 scale = new Vector3();
    Vector3 _origin = new Vector3();
    Vector3 _origin2 = new Vector3();

    RaycastHit hit = new RaycastHit();
    RaycastHit hit2 = new RaycastHit();
    // Update is called once per frame
    void Update ()
	{
		
		_origin = player.position;
        if (c != null) _origin.y = _origin.y + (c.center.y*c.transform.localScale.y) - c.bounds.extents.y + 0.1f;
        if (sc != null) _origin.y -= (sc.bounds.extents.y - 0.1f);
        _origin2 = _origin;
        _origin2.y += 0.25f;
        // If on a moving platform then the check might not work or miss the ground and show up below the platform, so
        // do a second cast slightly higher, if the hit points are too far apart, then hide the shadow
        if (!MediumControl.doSpinOutAfterNano && Physics.Raycast(_origin,Vector3.down,out hit,100f, ~(1 << 21),QueryTriggerInteraction.Ignore)) {


            float distance = Vector3.Distance(_origin, hit.point);

            if (Physics.Raycast(_origin2, Vector3.down, out hit2, 100f, ~(1 << 21), QueryTriggerInteraction.Ignore)) {
                if (Vector3.Distance(hit.point, hit2.point) > 0.5f) { 
                    t_fadeInTime = 0;
                    mr.enabled = false;
                    return;
                }
            }

            col = mat.color;
            col.a = Mathf.Lerp(alphaAtMin, alphaAtMax, distance / maxShadowDistance);
            t_fadeInTime += Time.deltaTime; if (t_fadeInTime > fadeInTime) t_fadeInTime = fadeInTime;
            col.a = Mathf.Lerp(0, col.a, t_fadeInTime / fadeInTime);

            if (distance < 1.75f) {
                col.a = Mathf.Lerp(0, col.a, distance / 1.75f);
                if (distance < 0.2f) {
                    col.a = 0;
                }
            }

            mat.color = col;


            scale = transform.localScale;
            float newScale = Mathf.Lerp(maxShadowScale, minShadowScale, distance / maxShadowDistance);
            scale.Set(newScale, newScale, newScale);
            transform.localScale = scale;


			_origin = hit.point;
            
			_origin.y += floorOffset;
			transform.position = _origin;

            transform.LookAt(transform.position + hit.normal*-1);

            mr.enabled = true;
		} else {
            t_fadeInTime = 0;
			mr.enabled = false;
		}


	}
}

