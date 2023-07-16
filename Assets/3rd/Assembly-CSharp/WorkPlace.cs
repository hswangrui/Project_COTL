using System.Collections.Generic;

public class WorkPlace : BaseMonoBehaviour
{
	public delegate void JobDelegate();

	public delegate void JobEnded(int WorkerCount);

	public static List<WorkPlace> workplaces = new List<WorkPlace>();

	public string ID;

	public List<WorkPlaceSlot> Positions = new List<WorkPlaceSlot>();

	private List<TaskDoer> Workers = new List<TaskDoer>();

	public List<Worshipper> Worshippers = new List<Worshipper>();

	private bool init;

	public static string NO_JOB = "-1";

	public StructureBrain.TYPES Type;

	public Task_Type JobType;

	public Structure structure;

	public JobDelegate OnJobBegin;

	public JobDelegate OnArrivedAtJob;

	public JobEnded OnJobEnded;

	private float PowerTimer;

	private void Start()
	{
		structure = GetComponent<Structure>();
		for (int i = 0; i < Positions.Count; i++)
		{
			Workers.Add(null);
		}
	}

	public void BeginJob(TaskDoer Worker, int Position)
	{
		Workers[Position] = Worker;
		Worshipper item = Worker as Worshipper;
		Worshippers.Add(item);
		JobDelegate onJobBegin = OnJobBegin;
		if (onJobBegin != null)
		{
			onJobBegin();
		}
	}

	public void ArrivedAtJob()
	{
		JobDelegate onArrivedAtJob = OnArrivedAtJob;
		if (onArrivedAtJob != null)
		{
			onArrivedAtJob();
		}
	}

	public void ClearAllWorkers()
	{
		int num = -1;
		while (++num < Worshippers.Count)
		{
			Worshipper.ClearJob(Worshippers[num]);
		}
		Worshippers.Clear();
		num = -1;
		while (++num < Workers.Count)
		{
			Workers[num] = null;
		}
	}

	public void EndJob(TaskDoer Worker, int Position)
	{
		Workers[Position] = null;
		Worshipper item = Worker as Worshipper;
		Worshippers.Remove(item);
		int num = 0;
		foreach (TaskDoer worker in Workers)
		{
			if (worker != null)
			{
				num++;
			}
		}
		if (OnJobEnded != null)
		{
			OnJobEnded(num);
		}
	}

	public bool HasPower()
	{
		return true;
	}

	private void Update()
	{
	}

	private void OnEnable()
	{
		workplaces.Add(this);
	}

	private void OnDisable()
	{
		workplaces.Remove(this);
	}

	public void SetID(string ID)
	{
		this.ID = ID;
	}

	public static WorkPlace GetWorkPlaceByID(string ID)
	{
		foreach (WorkPlace workplace in workplaces)
		{
			if (workplace.ID == ID)
			{
				return workplace;
			}
		}
		return null;
	}
}
