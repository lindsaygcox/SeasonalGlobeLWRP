using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

//based off https://github.com/SamMurphy/L-System-Trees-in-Unity/blob/master/Assets/Scripts/FractalTree.cs
//https://en.wikipedia.org/wiki/L-system

public sealed class Tree : MonoBehaviour
{
    private class PointData
    {
        public Vector3 Point;
        public Vector3 Angle;
        public float BranchLength;
        public PointData(Vector3 point, Vector3 angle, float branchLength)
        {
            Point = point;
            Angle = angle;
            BranchLength = branchLength;
        }
    }

    private readonly Dictionary<char, string> _rules = new Dictionary<char, string>();
    private readonly List<PointData> _points = new List<PointData>();
    private readonly List<GameObject> _branches = new List<GameObject>();

    public string Input = "F";
    private string _output;
    private string _result;

    [Range(0, 6)]
    [SerializeField]
    private int _iterations;

    [SerializeField]
    private GameObject _branch;

    [SerializeField]
    private Transform _parent;

    private void Awake()
    {
        GenerateTree();
    }

    private void GenerateTree()
    {
        Clear();
        SetupRulesDic();
        ApplyRulesForIterations();
        _result = _output;
        DeterminePoints(_output);
        CreateBranches();
    }

    private void Clear()
    {
        _rules.Clear();
        _points.Clear();
        _branches.ForEach(x => Destroy(x.gameObject));
        _branches.Clear();
    }

    private void SetupRulesDic()
    {
        // Rules
        // Key is replaced by value
        _rules.Add('F', "F[-F]F[+F][F]");
    }

    private void ApplyRulesForIterations()
    {
        //Apply the rules for number of iterations
        _output = Input;
        for (int i = 0; i < _iterations; ++i)
        {
            _output = ApplyRules(_output);
        }
    }

    private string ApplyRules(string input)
    {
        var sb = new StringBuilder();

        // Loop through characters in the input string
        foreach (char c in input)
        {
            // If character matches key in rules, then replace character with rhs of rule
            if (_rules.ContainsKey(c))
            {
                sb.Append(_rules[c]);
            }
            // If not, keep the character
            else
            {
                sb.Append(c);
            }
        }
        // Return string with rules applied
        return sb.ToString();
    }

    private void DeterminePoints(string input)
    {
        var returnValues = new Stack<PointData>();
        PointData lastPoint = new PointData(Vector3.zero, Vector3.zero, 1f);
        returnValues.Push(lastPoint);

        foreach (char c in input)
        {
            switch (c)
            {
                case 'F': // Draw line of length lastBranchLength, in direction of lastAngle
                    {
                        _points.Add(lastPoint);

                        var newPoint = new PointData(lastPoint.Point + new Vector3(0, lastPoint.BranchLength, 0), lastPoint.Angle, 1f);
                        newPoint.BranchLength = lastPoint.BranchLength - 0.02f;
                        if (newPoint.BranchLength <= 0.0f) newPoint.BranchLength = 0.001f;

                        newPoint.Angle.y = lastPoint.Angle.y + UnityEngine.Random.Range(-30, 30);

                        newPoint.Point = Pivot(newPoint.Point, lastPoint.Point, new Vector3(newPoint.Angle.x, 0, 0));
                        newPoint.Point = Pivot(newPoint.Point, lastPoint.Point, new Vector3(0, newPoint.Angle.y, 0));

                        _points.Add(newPoint);
                        lastPoint = newPoint;
                        break;
                    }
                case '+': // Rotate +30
                    {
                        lastPoint.Angle.x += 30.0f;
                        break;
                    }
                case '[': // Save State
                    {
                        returnValues.Push(lastPoint);
                        break;
                    }
                case '-': // Rotate -30
                    {
                        lastPoint.Angle.x += -30.0f;
                        break;
                    }
                case ']': // Load Saved State
                    {
                        lastPoint = returnValues.Pop();
                        break;
                    }
            }
        }
    }

    private Vector3 Pivot(Vector3 point1, Vector3 point2, Vector3 angles)
    {
        Vector3 dir = point1 - point2;
        dir = Quaternion.Euler(angles) * dir;
        point1 = dir + point2;
        return point1;
    }

    private void CreateBranches()
    {
        for (int i = 0; i < _points.Count; i += 2)
        {
            CreateBranch(_points[i], _points[i + 2], 0.1f);
        }
    }

    private void CreateBranch(PointData point1, PointData point2, float radius)
    {
        GameObject newCylinder = Instantiate(_branch, _parent);
        float length = Vector3.Distance(point2.Point, point1.Point);
        radius = radius * length;

        Vector3 scale = new Vector3(radius, length / 2.0f, radius);
        newCylinder.transform.localScale = scale;

        newCylinder.transform.position = point1.Point;
        newCylinder.transform.Rotate(point2.Angle);

        newCylinder.transform.parent = this.transform;

        _branches.Add(newCylinder);
    }

}
