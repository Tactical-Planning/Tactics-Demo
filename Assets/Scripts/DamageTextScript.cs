using UnityEngine;

public class DamageTextScript : MonoBehaviour {
	
	
	private TextMesh textMesh;
	private Vector3 destination;
	private Vector3 startPos;
	public int damage;
	public float lifeSpan;
	public float startTime;
	
	void Awake(){
		textMesh = gameObject.GetComponent<TextMesh>();
		lifeSpan = .5f;
		
	}
	
	void Start(){
		textMesh.text = damage.ToString();
		startTime = Time.time;
		destination = transform.position;
		destination.y +=lifeSpan;
		startPos = transform.position;
		
	}
	
	void Update(){
		//float the text up from the unit that took damage.
		float fracDistance = (Time.time - startTime)*.5f/lifeSpan;
		float fracAlpha = (Time.time - startTime)*.05f/lifeSpan;
		
		if(transform.position.y < destination.y){
			transform.position = Vector3.Lerp(startPos, destination, fracDistance);
			Color temp = textMesh.color;
			temp.a = Mathf.Lerp(temp.a, 0f, fracAlpha);
			textMesh.color = temp;
		}else{
			Destroy(gameObject);
		}
		
		
	}
	
	
	
}