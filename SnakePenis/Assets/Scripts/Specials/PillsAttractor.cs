using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PillsAttractor : MonoBehaviour
{

    private List<GameObject> PillsList;
    private LayerMask PillsMask;

    public GameObject AttractorObj;
    public ParticleSystem WindParticleSystem;
    [Header("Settings")]
    public float ActionRadius = 5f;
    public float MinSqrDistanceToRelease = 1f;
    public float AttractionForce = 10f;
    public float RotationSpeed = 2f;

    // Start is called before the first frame update
    void Start()
    {
        PillsList = new List<GameObject>();
        PillsMask = LayerMask.GetMask("Pills");
        StartCoroutine(CapturePillsCoroutine());
    }

    IEnumerator CapturePillsCoroutine()
    {
        while (true)
        {
            // Add Pills to captured pills list when in Action Radius
            Collider[] hitColliders = Physics.OverlapSphere(transform.position, ActionRadius, PillsMask);

            foreach (Collider col in hitColliders)
            {
                if (PillsList.Contains(col.gameObject) == false && IsPillValidToAttract(col.gameObject))
                {
                    PillsList.Add(col.gameObject);
                }
            }
            //Attract Pills in List
            float sqrMagnitude = 0f;
            List<GameObject> PillsToRemove = new List<GameObject>();
            Vector3 positionOnGround = transform.position;
            positionOnGround.y = 0f;
            foreach (GameObject Pill in PillsList)
            {
                if (IsPillValidToAttract(Pill))
                {
                    sqrMagnitude = Vector3.SqrMagnitude(Pill.transform.position - transform.position);
                    
                    Pill.transform.position = Vector3.Lerp(Pill.transform.position, positionOnGround, AttractionForce * Time.deltaTime / sqrMagnitude);
                }
                if (IsPillValidToAttract(Pill) == false || sqrMagnitude < MinSqrDistanceToRelease || sqrMagnitude > ActionRadius * ActionRadius * 2f)
                {
                    PillsToRemove.Add(Pill);
                }
            }

            // Release Pills
            for (int i = 0; i<PillsToRemove.Count; i++)
            {
                PillsList.Remove(PillsToRemove[i]);
            }

            PillsToRemove.Clear();

            //Orientate Attractor towards attracted pills
            Vector3 averagePoint = Vector3.zero;
            if (PillsList.Count > 0)
            {
                foreach(GameObject Pill in PillsList)
                {
                    averagePoint += Pill.transform.position;
                }
                averagePoint /= PillsList.Count;
                if (WindParticleSystem.isPlaying == false)
                {
                    WindParticleSystem.Play();
                }

            } else
            {
                averagePoint = transform.position + transform.forward * 10f;
                WindParticleSystem.Stop();
            }

            // Rotate attractor
            AttractorObj.transform.rotation = Quaternion.Lerp(AttractorObj.transform.rotation, Quaternion.LookRotation(averagePoint - transform.position, Vector3.forward), RotationSpeed * Time.deltaTime);
            yield return new WaitForEndOfFrame();
        }
    }

    bool IsPillValidToAttract(GameObject pill)
    {
        return pill != null && pill.GetComponent<Collider>().enabled;
    }
}
