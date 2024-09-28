using Data;
using System.Collections.Generic;
using UnityEngine;

public class Quest : MonoBehaviour
{
    public int Id { get; set; }
    public Dictionary<int, Script> Description { get; set; }
    public string Type { get; set; }
    public bool IsCompleted { get; set;}    
}