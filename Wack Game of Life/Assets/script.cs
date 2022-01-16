using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;
using Rnd = UnityEngine.Random;

public class script : MonoBehaviour {
    public KMBombInfo bomb;
    public KMAudio audio;

    static int ModuleIdCounter = 1;
    int ModuleId;
    private bool moduleSolved = false;

    public KMSelectable[] buttons;

    private int blackToAliveSquaresNum;
    private int[] whiteStillAliveRanges = new int[2];
    private bool[,] grids = new bool[4, 80];
    private bool[] inputGrid = new bool[48];
    private bool[] placeholderGrid = new bool[80];
    private List<bool> solutionsList = new List<bool>();
    private int[] gridNums = new int[48] { 9, 10, 11, 12, 13, 14, 17, 18, 19, 20, 21, 22, 25, 26, 27, 28, 29, 30, 33, 34, 35, 36, 37, 38, 41, 42, 43, 44, 45, 46, 49, 50, 51, 52, 53, 54, 57, 58, 59, 60, 61, 62, 65, 66, 67, 68, 69, 70 };
    private int blackPercent = 34;
    private int[] squareCheckNums = new int[8] { -9, -8, -7, -1, 1, 7, 8, 9 };
    private int whiteNeighbors = 0;
    private bool entireGridBlack = false;

    private int[] multipleCheckNums1 = new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 2, 2, 2, 2, 2, 2, 2, 3, 3, 3, 3, 3, 3, 4, 4, 4, 4, 4, 5, 5, 5, 5, 6, 6, 6, 7, 7, 8 };
    private int[] multipleCheckNums2 = new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 1, 2, 3, 4, 5, 6, 7, 8, 2, 3, 4, 5, 6, 7, 8, 3, 4, 5, 6, 7, 8, 4, 5, 6, 7, 8, 5, 6, 7, 8, 6, 7, 8, 7, 8, 8 };

    private List<int> secondGridOtherSolutions1 = new List<int>();
    private List<int> secondGridOtherSolutions2 = new List<int>();
    private List<int> debugOtherSolutions = new List<int>();

    public Renderer[] squares;
    public Material[] squareMats;

    private int intPlaceholder = 0;
    private bool viewingGrid2 = false;
    private bool viewingGrid3 = false;
    public TextMesh bigButtonText;

    private string[] debugGrids = new string[5];
    private string debugInput;

    private int tpWhichColumn = 0;
    private bool tpNoLetter = false;
    private bool tpStillFindingSolution = true;
    // Use this for initialization
    void Start()
    {
    initialization:
        solutionsList.Clear();
        debugGrids[0] = "";
        debugGrids[1] = "";
        debugGrids[2] = "";
        blackToAliveSquaresNum = Rnd.Range(0,9); //randomly select rules
        whiteStillAliveRanges[0] = Rnd.Range(0,9);
        whiteStillAliveRanges[1] = Rnd.Range(0,9);
        if (whiteStillAliveRanges[0] > whiteStillAliveRanges[1]) //if first value is bigger than second switch them
        {
            intPlaceholder = whiteStillAliveRanges[0];
            whiteStillAliveRanges[0] = whiteStillAliveRanges[1];
            whiteStillAliveRanges[1] = intPlaceholder;
        }
        for (int i = 0; i < 48; i++) //generate board
        {
            intPlaceholder = Rnd.Range(0, 48);
            if (intPlaceholder > blackPercent)
            {
                grids[0, gridNums[i]] = true;
                squares[i].material = squareMats[1];
                inputGrid[i] = true;
                debugGrids[0] = debugGrids[0] + "#";
            }
            else
            {
                grids[0, gridNums[i]] = false;
                squares[i].material = squareMats[0];
                inputGrid[i] = false;
                debugGrids[0] = debugGrids[0] + "O";
            }
        }
        for (int i = 0; i < 48; i++) //2nd generation
        {
            whiteNeighbors = 0;
            for (int j = 0; j < 8; j++)
            {
                if (grids[0, gridNums[i] + squareCheckNums[j]]) //if this square that we're checking is white
                {
                    whiteNeighbors++;
                }
            }
            if (grids[0, gridNums[i]]) //if this square is white
            {
                if (whiteNeighbors < whiteStillAliveRanges[0] || whiteNeighbors > whiteStillAliveRanges[1])
                {
                    grids[1, gridNums[i]] = false;
                    debugGrids[1] = debugGrids[1] + "O";
                }
                else
                {
                    grids[1, gridNums[i]] = true;
                    debugGrids[1] = debugGrids[1] + "#";
                }
            }
            else
            {
                if (whiteNeighbors == blackToAliveSquaresNum)
                {
                    grids[1, gridNums[i]] = true;
                    debugGrids[1] = debugGrids[1] + "#";
                }
                else
                {
                    grids[1, gridNums[i]] = false;
                    debugGrids[1] = debugGrids[1] + "O";
                }
            }
        }
        for (int i = 0; i < 48; i++) //3rd generation
        {
            whiteNeighbors = 0;
            for (int j = 0; j < 8; j++)
            {
                if (grids[1, gridNums[i] + squareCheckNums[j]]) //if this square that we're checking is white
                {
                    whiteNeighbors++;
                }
            }
            if (grids[1, gridNums[i]]) //if this square is white
            {
                if (whiteNeighbors < whiteStillAliveRanges[0] || whiteNeighbors > whiteStillAliveRanges[1])
                {
                    grids[2, gridNums[i]] = false;
                    debugGrids[2] = debugGrids[2] + "O";
                }
                else
                {
                    grids[2, gridNums[i]] = true;
                    debugGrids[2] = debugGrids[2] + "#";
                }
            }
            else
            {
                if (whiteNeighbors == blackToAliveSquaresNum)
                {
                    grids[2, gridNums[i]] = true;
                    debugGrids[2] = debugGrids[2] + "#";
                }
                else
                {
                    grids[2, gridNums[i]] = false;
                    debugGrids[2] = debugGrids[2] + "O";
                }
            }
        }
        debugGrids[4] = "";
        for (int i = 0; i < 48; i++) //4th generation (intended solution)
        {
            whiteNeighbors = 0;
            for (int j = 0; j < 8; j++)
            {
                if (grids[2, gridNums[i] + squareCheckNums[j]]) //if this square that we're checking is white
                {
                    whiteNeighbors++;
                }
            }
            if (grids[2, gridNums[i]]) //if this square is white
            {
                if (whiteNeighbors < whiteStillAliveRanges[0] || whiteNeighbors > whiteStillAliveRanges[1])
                {
                    debugGrids[4] = debugGrids[4] + "O";
                }
                else
                {
                    debugGrids[4] = debugGrids[4] + "#";
                }
            }
            else
            {
                if (whiteNeighbors == blackToAliveSquaresNum)
                {
                    debugGrids[4] = debugGrids[4] + "#";
                }
                else
                {
                    debugGrids[4] = debugGrids[4] + "O";
                }
            }
        }
        entireGridBlack = true;
        for (int i = 0; i < 48; i++) //check if boards 1 and 2 or boards 2 and 3 are the same, or if any of the stages are entirely black for more interesting modules
        {
            if (grids[0, gridNums[i]] == false && entireGridBlack)
            {
                if (i == 47)
                {
                    goto initialization;
                }
            }
            else
            {
                entireGridBlack = false;
            }
            if (grids[0, gridNums[i]] == grids[1, gridNums[i]])
            {
                if (i == 47)
                {
                    goto initialization;
                }
            }
            else
            {
                i = 48;
            }
        }
        entireGridBlack = true;
        for (int i = 0; i < 48; i++) //check if boards 1 and 2 or boards 2 and 3 are the same, or if any of the stages are entirely black for more interesting modules
        {
            if (grids[1, gridNums[i]] == false && entireGridBlack)
            {
                if (i == 47)
                {
                    goto initialization;
                }
            }
            else
            {
                entireGridBlack = false;
            }
            if (grids[1, gridNums[i]] == grids[2, gridNums[i]])
            {
                if (i == 47)
                {
                    goto initialization;
                }
            }
            else
            {
                i = 48;
            }
        }
        entireGridBlack = true;
        for (int i = 0; i < 48; i++) //check if boards 1 and 2 or boards 2 and 3 are the same, or if any of the stages are entirely black for more interesting modules
        {
            if (grids[2, gridNums[i]] == false && entireGridBlack)
            {
                if (i == 47)
                {
                    goto initialization;
                }
            }
            else
            {
                entireGridBlack = false;
                i = 48;
            }
        }
        for (int g = 0; g < 45; g++)
        {
            for (int h = 0; h < 9; h++)
            {
                for (int i = 0; i < 48; i++) //3rd generation
                {
                    whiteNeighbors = 0;
                    for (int j = 0; j < 8; j++)
                    {
                        if (grids[0, gridNums[i] + squareCheckNums[j]]) //if this square that we're checking is white
                        {
                            whiteNeighbors++;
                        }
                    }
                    if (grids[0, gridNums[i]]) //if this square is white
                    {
                        if (whiteNeighbors < multipleCheckNums1[g] || whiteNeighbors > multipleCheckNums2[g])
                        {
                            placeholderGrid[gridNums[i]] = false;
                        }
                        else
                        {
                            placeholderGrid[gridNums[i]] = true;
                        }
                    }
                    else
                    {
                        if (whiteNeighbors == h)
                        {
                            placeholderGrid[gridNums[i]] = true;
                        }
                        else
                        {
                            placeholderGrid[gridNums[i]] = false;
                        }
                    }
                }
                for (int i = 0; i < 48; i++)
                {
                    if (placeholderGrid[gridNums[i]] != grids[1, gridNums[i]])
                    {
                        i = 48;
                    }
                    else if (i == 47)
                    {
                        secondGridOtherSolutions1.Add(g);
                        secondGridOtherSolutions2.Add(h);
                    }
                }
            }
        }
        if (secondGridOtherSolutions1.Count > 0)
        {
            for (int k = 0; k < secondGridOtherSolutions1.Count; k++)
            {
                for (int i = 0; i < 48; i++) //3rd generation
                {
                    whiteNeighbors = 0;
                    for (int j = 0; j < 8; j++)
                    {
                        if (grids[1, gridNums[i] + squareCheckNums[j]]) //if this square that we're checking is white
                        {
                            whiteNeighbors++;
                        }
                    }
                    if (grids[1, gridNums[i]]) //if this square is white
                    {
                        if (whiteNeighbors < multipleCheckNums1[secondGridOtherSolutions1[k]] || whiteNeighbors > multipleCheckNums2[secondGridOtherSolutions1[k]])
                        {
                            placeholderGrid[gridNums[i]] = false;
                        }
                        else
                        {
                            placeholderGrid[gridNums[i]] = true;
                        }
                    }
                    else
                    {
                        if (whiteNeighbors == secondGridOtherSolutions2[k])
                        {
                            placeholderGrid[gridNums[i]] = true;
                        }
                        else
                        {
                            placeholderGrid[gridNums[i]] = false;
                        }
                    }
                }
                for (int i = 0; i < 48; i++)
                {
                    if (placeholderGrid[gridNums[i]] != grids[2, gridNums[i]])
                    {
                        i = 48;
                    }
                    else if (i == 47)
                    {
                        unintendedSolution(k);
                        debugOtherSolutions.Add(k);
                    }
                }
            }
        }
        string debugExtraSolutions = "\n";
        for (int i = 0; i < debugOtherSolutions.Count; i++)
        {
            debugExtraSolutions += multipleCheckNums1[secondGridOtherSolutions1[debugOtherSolutions[i]]] + "-" + multipleCheckNums2[secondGridOtherSolutions1[debugOtherSolutions[i]]] + ", " + secondGridOtherSolutions2[debugOtherSolutions[i]] + "\n";
        }

        DebugMsg("Grid one:");
        LogGrids(debugGrids[0]);
        DebugMsg("Grid two:");
        LogGrids(debugGrids[1]);
        DebugMsg("Grid three:");
        LogGrids(debugGrids[2]);

        DebugMsg("All solutions the computer could find, in the format \"Minimum White Neighbors-Maximum White Neighbors for a white cell to be kept alive, Number of White Neighbors to turn a black square white\"." + debugExtraSolutions);

        DebugMsg("Intended number of neighbouring squares to turn a black square white: " + blackToAliveSquaresNum);
        DebugMsg("Intended range of neighbouring squares to keep a white square white: " + whiteStillAliveRanges[0] + " is the minimum number of squares, " + whiteStillAliveRanges[1] + " is the maximum number of squares.");

        DebugMsg("Intended solution:");
        LogGrids(debugGrids[4]);
    }
    
    void unintendedSolution(int k)
    {
        for (int i = 0; i < 48; i++) //3rd generation
        {
            whiteNeighbors = 0;
            for (int j = 0; j < 8; j++)
            {
                if (grids[2, gridNums[i] + squareCheckNums[j]]) //if this square that we're checking is white
                {
                    whiteNeighbors++;
                }
            }
            if (grids[2, gridNums[i]]) //if this square is white
            {
                if (whiteNeighbors < multipleCheckNums1[secondGridOtherSolutions1[k]] || whiteNeighbors > multipleCheckNums2[secondGridOtherSolutions1[k]])
                {
                    placeholderGrid[gridNums[i]] = false;
                }
                else
                {
                    placeholderGrid[gridNums[i]] = true;
                }
            }
            else
            {
                if (whiteNeighbors == secondGridOtherSolutions2[k])
                {
                    placeholderGrid[gridNums[i]] = true;
                }
                else
                {
                    placeholderGrid[gridNums[i]] = false;
                }
            }
        }
        for(int i = 0; i < 48; i++)
        {
            solutionsList.Add(placeholderGrid[gridNums[i]]);
        }
        tpStillFindingSolution = false;
    }

	// Update is called once per frame
	void Awake () {
        ModuleId = ModuleIdCounter++;

        foreach (KMSelectable button in buttons)
        {
            KMSelectable pressedButton = button;
            button.OnInteract += delegate () { buttonPressed(pressedButton); return false; };
        }
    }

    void buttonPressed(KMSelectable pressedButton)
    {
        pressedButton.AddInteractionPunch();
        GetComponent<KMAudio>().PlayGameSoundAtTransformWithRef(KMSoundOverride.SoundEffect.ButtonPress, transform);

        if (moduleSolved)
        {
            return;
        }

        pressedButton.AddInteractionPunch();
        GetComponent<KMAudio>().PlayGameSoundAtTransformWithRef(KMSoundOverride.SoundEffect.ButtonPress, transform);

        if (pressedButton == buttons[0])
        {
            if (viewingGrid2)
            {
                for (int i = 0; i < 48; i++)
                {
                    if (grids[2,gridNums[i]] == true)
                    {
                        squares[i].material = squareMats[1];
                        inputGrid[i] = true;
                    }
                    else
                    {
                        squares[i].material = squareMats[0];
                        inputGrid[i] = false;
                    }
                }
                viewingGrid2 = false;
                viewingGrid3 = true;
                bigButtonText.text = "3";
            }
            else if(viewingGrid3)
            {
                for (int i = 0; i < 48; i++)
                {
                    if (grids[0,gridNums[i]] == true)
                    {
                        squares[i].material = squareMats[1];
                        inputGrid[i] = true;
                    }
                    else
                    {
                        squares[i].material = squareMats[0];
                        inputGrid[i] = false;
                    }
                }
                viewingGrid2 = false;
                viewingGrid3 = false;
                bigButtonText.text = "1";
            }
            else
            {
                for (int i = 0; i < 48; i++)
                {
                    if (grids[1,gridNums[i]] == true)
                    {
                        squares[i].material = squareMats[1];
                        inputGrid[i] = true;
                    }
                    else
                    {
                        squares[i].material = squareMats[0];
                        inputGrid[i] = false;
                    }
                }
                viewingGrid2 = true;
                viewingGrid3 = false;
                bigButtonText.text = "2";
            }
            return;
        }
        else if(pressedButton == buttons[1])
        {
            for(int i = 0; i < 48; i++)
            {
                squares[i].material = squareMats[0];
                inputGrid[i] = false;
            }
        }
        else if(pressedButton == buttons[2])
        {
            debugGrids[3] = "";
            for(int i = 0; i < 48; i++)
            {
                if(inputGrid[i])
                {
                    debugGrids[3] = debugGrids[3] + '#';
                }
                else
                {
                    debugGrids[3] = debugGrids[3] + 'O';
                }
            }
            DebugMsg("Inputted:");
            LogGrids(debugGrids[3]);
            for (int i = 0; i < (solutionsList.Count / 48); i++)
            {
                for(int j = 0; j < 48; j++)
                {
                    if (inputGrid[j] != solutionsList[j + (i * 48)])
                    {
                        j = 48;
                    }
                    else if(j == 47)
                    {
                        DebugMsg("Correct input, module solved!");
                        moduleSolved = true;
                        GetComponent<KMBombModule>().HandlePass();
                        return;
                    }
                }
            }
            GetComponent<KMBombModule>().HandleStrike();
            DebugMsg("Strike! This input doesn't match any calculated solutions.");
        }
        else
        {
            for(int i = 0; i < 48; i++)
            {
                if(pressedButton == buttons[i + 3])
                {
                    if(inputGrid[i])
                    {
                        inputGrid[i] = false;
                        squares[i].material = squareMats[0];
                    }
                    else
                    {
                        inputGrid[i] = true;
                        squares[i].material = squareMats[1];
                    }
                }
            }
        }
    }

    private bool isCommandValid(string cmd)
    {
        string[] validbtns = { "a1", "a2", "a3", "a4", "a5", "a6", "a7", "a8", "b1", "b2", "b3", "b4", "b5", "b6", "b7", "b8", "c1", "c2", "c3", "c4", "c5", "c6", "c7", "c8", "d1", "d2", "d3", "d4", "d5", "d6", "d7", "d8", "e1", "e2", "e3", "e4", "e5", "e6", "e7", "e8", "f1", "f2", "f3", "f4", "f5", "f6", "f7", "f8","next","submit","clear" };

        string[] btnSequence = cmd.ToLowerInvariant().Split(new[] { ' ' });

        foreach (var btn in btnSequence)
        {
            if (!validbtns.Contains(btn.ToLower()))
            {
                return false;
            }
        }
        return true;
    }

    public string TwitchHelpMessage = "Use !{0} a1 b2 c3 to press a1, b2, and c3. Use !{0} clear to press the clear button. Use !{0} submit to submit the current configuration. Use !{0} next to view the next iteration, or the first if you're on the third iteration. All commands can be chained (Example: !{0} a1 clear b2 b3 submit)";
    IEnumerator ProcessTwitchCommand(string cmd)
    {
        var parts = cmd.ToLowerInvariant().Split(new[] { ' ' });

        if (isCommandValid(cmd))
        {
            yield return null;
            for (int i = 0; i < parts.Count(); i++)
            {
                if (parts[i] == "next")
                {
                    yield return new KMSelectable[] { buttons[0] };
                }
                else if (parts[i] == "clear")
                {
                    yield return new KMSelectable[] { buttons[1] };
                }
                else if (parts[i] == "submit")
                {
                    yield return new KMSelectable[] { buttons[2] };
                }
                else
                {
                    tpNoLetter = false;
                    switch (parts[i][0])
                    {
                        case 'a':
                            tpWhichColumn = 0;
                            break;
                        case 'b':
                            tpWhichColumn = 1;
                            break;
                        case 'c':
                            tpWhichColumn = 2;
                            break;
                        case 'd':
                            tpWhichColumn = 3;
                            break;
                        case 'e':
                            tpWhichColumn = 4;
                            break;
                        case 'f':
                            tpWhichColumn = 5;
                            break;
                        default:
                            tpNoLetter = true;
                            break;
                    }
                    if (tpNoLetter)
                    {
                        yield break;
                    }
                    else
                    {
                        for (int j = 0; j < 8; j++)
                        {
                            if (parts[i][1].ToString() == (j + 1).ToString())
                            {
                                yield return new KMSelectable[] { buttons[((j * 6) + tpWhichColumn) + 3] };
                                j = 8;
                            }
                            else if (j == 7)
                            {
                                yield break;
                            }
                        }
                    }
                }
            }
        }
        else
        {
            yield break;
        }
    }

    IEnumerator TwitchHandleForcedSolve()
    {
        yield return new WaitForSeconds(.05f);
        while(tpStillFindingSolution)
        {
            yield return new WaitForSeconds(.05f);
        }
        buttons[1].OnInteract();
        for (int i = 0; i < 48; i++)
        {
            if(solutionsList[i]) //if the first solution's square is white
            {
                yield return new WaitForSeconds(.1f);
                buttons[i + 3].OnInteract();
            }
        }
        yield return new WaitForSeconds(.5f);
        buttons[2].OnInteract();
    }

    void LogGrids(string gridString) //stolen from life iteration lo
    {
        string logString = "\n";
        for (int i = 0; i < 48; i++)
        {
            logString = logString + gridString[i];
            if ((i + 1) % 6 == 0) logString += "\n";
        }
        if(!Application.isEditor) 
        {
            logString.Replace("#", "◼").Replace("O", "◻");
        }
        DebugMsg(logString);
    }

    void DebugMsg(string msg)
    {
        Debug.LogFormat("[Wack Game of Life #{0}] {1}", ModuleId, msg);
    }
}
