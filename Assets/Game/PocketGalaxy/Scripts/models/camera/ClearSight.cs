using UnityEngine;
using System.Collections;

public class ClearSight : MonoBehaviour
{
	public float DistanceToPlayer;
	void FixedUpdate()
	{
		Collider[] hits;
		// you can also use CapsuleCastAll()
		// TODO: setup your layermask it improve performance and filter your hits.
		hits = Physics.OverlapSphere(transform.position, DistanceToPlayer);
		foreach(Collider hit in hits)
		{
			if(hit.transform.GetComponent<AllowTransparency> () != null){
			Renderer R = hit.GetComponent<Collider>().GetComponent<Renderer>();
			if (R == null)
				continue; // no renderer attached? go to next hit
			// TODO: maybe implement here a check for GOs that should not be affected like the player


			AutoTransparent AT = R.GetComponent<AutoTransparent>();
			if (AT == null) // if no script is attached, attach one
			{
				AT = R.gameObject.AddComponent<AutoTransparent>();
			}
			AT.BeTransparent(Time.deltaTime); // get called every frame to reset the falloff
			}
		}
	}
}

public class AutoTransparent : MonoBehaviour
{
	private Shader m_OldShader = null;
	private float m_Transparency = 1f;
	private const float m_TargetTransparancy = 0.2f;
	private const float m_FallOff = 1f; // returns to 100% in 0.1 sec

	private bool transparent;


	public void BeTransparent(float time)
	{
			transparent = true;
			if (m_Transparency > m_TargetTransparancy)
				m_Transparency -= time * 2;
			if (m_OldShader == null) {
				// Save the current shader
				m_OldShader = GetComponent<Renderer> ().material.shader;
				foreach (Material m in GetComponent<Renderer>().materials)
					m.shader = Shader.Find ("Transparent/Diffuse");
			}
	}
	void FixedUpdate()
	{
		if (m_Transparency < m_FallOff)
		{
			foreach (Material m in GetComponent<Renderer>().materials) {
				Color C = m.color;
				C.a = m_Transparency;
				m.color = C;
			}
		}
		else
		{
			// Reset the shader
			GetComponent<Renderer>().material.shader = m_OldShader;
			// And remove this script
			Destroy(this);
		}
		if(!transparent)
			m_Transparency += Time.deltaTime * 2;
		else
			transparent = false;
	}

}