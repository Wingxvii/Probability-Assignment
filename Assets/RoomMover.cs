using System.Collections.Generic;
using UnityEngine;

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
    }

    //Returns list of door types calculated from probability dictionary
    private List<DoorType> Statistics(Dictionary<DoorType, float> prob, float probSum = 1.0f)
    {
        List<DoorType> doors = new List<DoorType>();

        //temp prob ensures origin prob stays intact
        Dictionary<DoorType, float> tempProb = new Dictionary<DoorType, float>();

        //loop through all probabilities above door fraction threshold
        foreach (KeyValuePair<DoorType, float> entry in prob)
        {
            tempProb.Add(entry.Key, entry.Value);
            while (entry.Value >= (probSum / allrooms.Count))
            {
                tempProb[entry.Key] -= allrooms.Count;
                doors.Add(entry.Key);
            }
        }

        //flush remainder door probabilities until door count is full
        while (doors.Count < allrooms.Count)
        {
            //update probabilities
            doors.Add(RandWeightedItem(tempProb));
        }

        //handle probability not summing to expected 1
        if (doors.Count > allrooms.Count) {
            Debug.LogWarning("Probability did not sum to 1");

            float actualSum = 0.0f;
            //determine actual sum
            foreach (KeyValuePair<DoorType, float> entry in prob)
            {
                while (entry.Value >= (probSum / allrooms.Count))
                {
                    actualSum += entry.Value;
                }
            }

            return Statistics(prob, actualSum);
        }

        return doors;
    }

    private DoorType RandWeightedItem(Dictionary<DoorType, float> prob)
    {
        float accumulatedWeight = 0.0f;

        //accumulate weight
        foreach (KeyValuePair<DoorType, float> entry in prob)
        {
            if (entry.Value > 0.0f)
            {
                accumulatedWeight += entry.Value;
            }
        }

        //pick
        float selectedValue = Random.Range(0, accumulatedWeight);

        //find
        foreach (KeyValuePair<DoorType, float> entry in prob)
        {
            if (entry.Value > 0.0f)
            {
                selectedValue -= entry.Value;

                if (selectedValue >= 0.0f)
                {
                    prob[entry.Key] = 0;
                    return entry.Key;
                }
            }
        }

        Debug.LogError("Accumulated Range was unexpected.");
        return DoorType.None;
    }
}
