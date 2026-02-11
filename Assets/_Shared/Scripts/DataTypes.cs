using System;
using System.Collections.Generic;

[Serializable]
public class Question
{
    public string text;
    public bool isYes;
}

[System.Serializable]
public class CategoryData
{
    public string categoryName;
    public string iconFileName; // Name of the image file
    public List<Question> questions;
}