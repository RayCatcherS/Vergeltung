using UnityEngine;
using System;
using System.Collections;


public class CableComponent : MonoBehaviour
{
	#region Class members

	[SerializeField] public Transform endPoint;
	[SerializeField] public Material cableMaterial;

	// Cable config
	[SerializeField] public float cableLength = 4;
	[SerializeField] public int totalSegments = 40;
	[SerializeField] public float segmentsPerUnit = 6f;
	public int segments = 0;
	[SerializeField] private float cableWidth = 0.1f;

	// Solver config
	[SerializeField] private int verletIterations = 1;
	[SerializeField] public int solverIterations = 10;

	//[Range(0,3)]
	[SerializeField] private float stiffness = 1f;

	public LineRenderer line;
	public CableParticle[] points;

    public void recalculate()
    {
        gameObject.transform.position = new Vector3(gameObject.transform.position.x-244, gameObject.transform.position.y, gameObject.transform.position.z);
        InitCableParticles();
        InitLineRenderer();
        /*
        CableParticle start = points[0];
        CableParticle end = points[segments];
        start.Bind(this.transform);
        end.Bind(endPoint.transform);

        for (int i = 0; i < segments+1; i++)
        {
            //points[i]._boundTo.transform.position = new Vector3(points[i]._boundTo.transform.position.x-244, points[i]._boundTo.transform.position.y, points[i]._boundTo.transform.position.z);

            //points[i]._boundRigid.velocity = Vector3.zero;
            points[i].Position = new Vector3(points[i].Position.x - 244, points[i].Position.y, points[i].Position.z);
            //points[i]._oldPosition = new Vector3(points[i].Position.x - 244, points[i].Position.y, points[i].Position.z);
            //points[i]._position = new Vector3(points[i].Position.x - 244, points[i].Position.y, points[i].Position.z);
            //points[i].UpdatePosition(new Vector3(points[i].Position.x - 244, points[i].Position.y, points[i].Position.z));
            //points[i].UpdateVerlet(new Vector3(points[i].Position.x - 244, points[i].Position.y, points[i].Position.z));

            //line.SetPosition(i,new Vector3(line.GetPosition(i).x - 244, line.GetPosition(i).y, line.GetPosition(i).z) );
            //InitCableParticles();
            //InitLineRenderer();
        }*/

        /*for (int i = 0; i <= segments; i++)
        {
            // Initial position
            //points[i].UpdatePosition(new Vector3(points[i].Position.x - 244, points[i].Position.y, points[i].Position.z));
            //points[i].UpdateVerlet(new Vector3(points[i].Position.x - 244, points[i].Position.y, points[i].Position.z));
            //points[i].Position = new Vector3(points[i].Position.x - 244, points[i].Position.y, points[i].Position.z);
            //points[i].Position = new Vector3(points[i].Position.x - 244, points[i].Position.y, points[i].Position.z);
            //points[i].Velocity = Vector3.zero;
            points[i] = new CableParticle(new Vector3 (points[i].Position.x-244, points[i].Position.y, points[i].Position.z));
            
        }*/
        // Bind start and end particles with their respective gameobjects


        //gameObject.GetComponent<CableComponent>().enabled = false;
    }
    #endregion


    #region Initial setup

    void Start()
	{
        
		InitCableParticles();
		InitLineRenderer();
	}
    public void inizialize()
    {
        InitCableParticles();
        InitLineRenderer();
    }
    /**
	 * Init cable particles
	 * 
	 * Creates the cable particles along the cable length
	 * and binds the start and end tips to their respective game objects.
	 */
    public void InitCableParticles()
	{
		// Calculate segments to use
		if (totalSegments > 0)
			segments = totalSegments;
		else
			segments = Mathf.CeilToInt (cableLength * segmentsPerUnit);

		Vector3 cableDirection = (endPoint.localPosition - transform.localPosition).normalized;
		float initialSegmentLength = cableLength / segments;
		points = new CableParticle[segments + 1];

		// Foreach point
		for (int pointIdx = 0; pointIdx <= segments; pointIdx++) {
			// Initial position
			Vector3 initialPosition = transform.position + (cableDirection * (initialSegmentLength * pointIdx));
			points[pointIdx] = new CableParticle(initialPosition);
		}

		// Bind start and end particles with their respective gameobjects
		CableParticle start = points[0];
		CableParticle end = points[segments];
        end.Bind(endPoint.transform);
        start.Bind(this.transform);
		
	}

	/**
	 * Initialized the line renderer
	 */
	public void InitLineRenderer()
	{
        
        line = gameObject.GetComponent<LineRenderer>();
        
		
		line.SetWidth(cableWidth, cableWidth);
		line.SetVertexCount(segments + 1);
		line.material = cableMaterial;
		line.GetComponent<Renderer>().enabled = true;
	}

	#endregion


	#region Render Pass

	void Update()
	{
		RenderCable();
	}

	/**
	 * Render Cable
	 * 
	 * Update every particle position in the line renderer.
	 */
	public void RenderCable()
	{
		for (int pointIdx = 0; pointIdx < segments + 1; pointIdx++) 
		{
			line.SetPosition(pointIdx, points [pointIdx].Position);
		}
	}

	#endregion


	#region Verlet integration & solver pass

	void FixedUpdate()
	{
        
        for (int verletIdx = 0; verletIdx < verletIterations; verletIdx++) 
		{
			VerletIntegrate();
			SolveConstraints();
		}
	}

	/**
	 * Verler integration pass
	 * 
	 * In this step every particle updates its position and speed.
	 */
	public void VerletIntegrate()
	{
		Vector3 gravityDisplacement = Time.fixedDeltaTime * Time.fixedDeltaTime * Physics.gravity;
		foreach (CableParticle particle in points) 
		{
			particle.UpdateVerlet(gravityDisplacement);
		}
	}

	/**
	 * Constrains solver pass
	 * 
	 * In this step every constraint is addressed in sequence
	 */
	public void SolveConstraints()
	{
		// For each solver iteration..
		for (int iterationIdx = 0; iterationIdx < solverIterations; iterationIdx++) 
		{
			SolveDistanceConstraint();
			SolveStiffnessConstraint();
		}
	}

	#endregion


	#region Solver Constraints

	/**
	 * Distance constraint for each segment / pair of particles
	 **/
	public void SolveDistanceConstraint()
	{
		float segmentLength = cableLength / segments;
		for (int SegIdx = 0; SegIdx < segments; SegIdx++) 
		{
			CableParticle particleA = points[SegIdx];
			CableParticle particleB = points[SegIdx + 1];

			// Solve for this pair of particles
			SolveDistanceConstraint(particleA, particleB, segmentLength);
		}
	}
		
	/**
	 * Distance Constraint 
	 * 
	 * This is the main constrains that keeps the cable particles "tied" together.
	 */
	public void SolveDistanceConstraint(CableParticle particleA, CableParticle particleB, float segmentLength)
	{
		// Find current vector between particles
		Vector3 delta = particleB.Position - particleA.Position;
		// 
		float currentDistance = delta.magnitude;
		float errorFactor = (currentDistance - segmentLength) / currentDistance;
		
		// Only move free particles to satisfy constraints
		if (particleA.IsFree() && particleB.IsFree()) 
		{
			particleA.Position += errorFactor * 0.5f * delta;
			particleB.Position -= errorFactor * 0.5f * delta;
		} 
		else if (particleA.IsFree()) 
		{
			particleA.Position += errorFactor * delta;
		} 
		else if (particleB.IsFree()) 
		{
			particleB.Position -= errorFactor * delta;
		}
	}

	/**
	 * Stiffness constraint
	 **/
	public void SolveStiffnessConstraint()
	{
		float distance = (points[0].Position - points[segments].Position).magnitude;
		if (distance > cableLength) 
		{
			foreach (CableParticle particle in points) 
			{
				SolveStiffnessConstraint(particle, distance);
			}
		}	
	}

	/**
	 * TODO: I'll implement this constraint to reinforce cable stiffness 
	 * 
	 * As the system has more particles, the verlet integration aproach 
	 * may get way too loose cable simulation. This constraint is intended 
	 * to reinforce the cable stiffness.
	 * // throw new System.NotImplementedException ();
	 **/
	void SolveStiffnessConstraint(CableParticle cableParticle, float distance)
	{
	

	}

	#endregion
}
