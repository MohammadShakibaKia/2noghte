using System.Collections.Generic;

[System.Serializable]
public class EmdadiData
{
    // The list of words to show in the center (e.g., "Song", "School", etc.)
    public List<string> words;
    
    // The list of 5 tasks on the right side (e.g., "Poem", "Joke", "Story")
    public List<string> initialTasks;
}