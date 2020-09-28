using System.Collections.Generic;
using UnityEngine;
using System.IO;

public enum DoorType { 
    HotNoisySafe = 0,
    HotNoisy = 1,
    HotSafe = 2,
    Hot = 3,
    NoisySafe = 4,
    Noisy = 5,
    Safe = 6,
    None = 7,

}

public class RoomMover : MonoBehaviour
{
    public List<Transform> allrooms = new List<Transform>();
    public GameObject treasurePrefab;
    public GameObject stonePrefab;
    public string fileName = "Assets/probabilities.txt";

    public List<DoorType> allDoorsDebug;
    public List<double> probDebug;

    // Start is called before the first frame update
    void Start()
    {
        int iterator = 0;
        foreach (Transform transform in this.GetComponentInChildren<Transform>())
        {
            allrooms.Add(transform);

            transform.rotation = Quaternion.Euler(0, 18 * iterator, 0);
            float angle = (18 * iterator) * Mathf.Deg2Rad;
            transform.position = new Vector3(-Mathf.Cos(angle) * 6, 0.25f, Mathf.Sin(angle) * 6);

            iterator++;
        }

        //call setup
        ParseDoors(fileName);
    }

    //Parses probabilities from text file and sends to statistics
    private void ParseDoors(string fileName) {
        Dictionary<DoorType, double> prob = new Dictionary<DoorType, double>();
        StreamReader reader = new StreamReader(fileName);
        
        //skip first line
        string line = reader.ReadLine();
        line = reader.ReadLine();

        while (line != null) {
            //remove all the whites
            line = line.Replace("/", string.Empty)
                .Replace(" ", string.Empty)
                .Replace("\t", string.Empty);

            string type = line.Substring(0, 3);
            double value = 0;

            if (!double.TryParse(line.Substring(3), out value))
            {
                Debug.LogError("Invalid Parsing: Unexpected Probability value format");
            }

            //fill dictionary based on doortype
            switch (type) {
                case "YYY":
                    prob.Add(DoorType.HotNoisySafe, value);
                    break;
                case "YYN":
                    prob.Add(DoorType.HotNoisy, value);
                    break;
                case "YNY":
                    prob.Add(DoorType.HotSafe, value);
                    break;
                case "YNN":
                    prob.Add(DoorType.Hot, value);
                    break;
                case "NYY":
                    prob.Add(DoorType.NoisySafe, value);
                    break;
                case "NYN":
                    prob.Add(DoorType.Noisy, value);
                    break;
                case "NNY":
                    prob.Add(DoorType.Safe, value);
                    break;
                case "NNN":
                    prob.Add(DoorType.None, value);
                    break;
                default:
                    Debug.LogError("Invalid Parsing: Unexpected Probability Type format");
                    break;
            }
            line = reader.ReadLine();
        }

        allDoorsDebug = Statistics(prob);
    }

    //sets up scene using input door list
    private void SetupDoors(List<DoorType> allDoors) {
    }

    //Returns list of door types calculated from probability dictionary
    private List<DoorType> Statistics(Dictionary<DoorType, double> prob, double probSum = 1.0)
    {
        List<DoorType> doors = new List<DoorType>();

        //temp prob ensures origin prob stays intact
        Dictionary<DoorType, double> tempProb = new Dictionary<DoorType, double>(prob);

        double doorPercentRatio = probSum / (double)allrooms.Count;

        //loop through all probabilities above door fraction threshold
        foreach (KeyValuePair<DoorType, double> entry in prob)
        {
            double probfract = entry.Value;

            while (probfract >= doorPercentRatio)
            {
                probfract -= doorPercentRatio;
                doors.Add(entry.Key);
            }

            //WTF why is double not precise? I thought that was the whole point??????? you better fix it billy
            probfract = Mathf.Round(((float)probfract) * 100f) / 100f;
            //update tempProb for use in flushing
            tempProb[entry.Key] = (double)probfract;
            probDebug.Add((double)probfract);
        }

        //flush remainder door probabilities until door count is full
        while (doors.Count < allrooms.Count)
        {
            //update probabilities
            DoorType toAdd = RandWeightedItem(tempProb);
            doors.Add(toAdd);
            //zero for the future
            tempProb[toAdd] = 0.0;
        }

        //handle probability not summing to expected 1
        if (doors.Count > allrooms.Count) {
            Debug.LogWarning("Probability did not sum to 1");

            double actualSum = 0.0;
            //determine actual sum
            foreach (KeyValuePair<DoorType, double> entry in prob)
            {
                actualSum += entry.Value;
            }

            return Statistics(prob, actualSum);
        }
        return doors;
    }

    //Returns weighted random from a dict list
    private DoorType RandWeightedItem(Dictionary<DoorType, double> prob)
    {

        double accumulatedWeight = 0.0;

        //accumulate weight
        foreach (KeyValuePair<DoorType, double> entry in prob)
        {
            if (entry.Value > 0.0)
            {
                accumulatedWeight += entry.Value;
            }
        }

        //pick
        double selectedValue = Random.Range(0f, (float)accumulatedWeight);
        Debug.Log(selectedValue);

        //find
        foreach (KeyValuePair<DoorType, double> entry in prob)
        {
            if (entry.Value > 0.0)
            {
                selectedValue -= entry.Value;

                if (selectedValue <= 0.0)
                {
                    return entry.Key;
                }
            }
        }

        Debug.LogError("Accumulated Range was unexpected.");
        return DoorType.None;
    }

    
}
