using UnityEngine;

public class CheckpointTrigger : MonoBehaviour
{
    public int checkpointIndex;
    public bool isFinishLine = false;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        LapManager lm = FindObjectOfType<LapManager>();
        if (lm == null) return;

        if (isFinishLine)
            lm.OnFinishLineCrossed();
        else
            lm.OnCheckpointCrossed(checkpointIndex);
    }
}