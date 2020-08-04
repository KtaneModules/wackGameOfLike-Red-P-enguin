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
    private bool[,] grids = new bool[4,80];
    private bool[] inputGrid = new bool[48];
    private bool[] placeholderGrid = new bool[80];
    private List<bool> solutionsList = new List<bool>();
    private int[] gridNums = new int[48] { 9,10,11,12,13,14,17,18,19,20,21,22,25,26,27,28,29,30,33,34,35,36,37,38,41,42,43,44,45,46,49,50,51,52,53,54,57,58,59,60,61,62,65,66,67,68,69,70 };
    private int blackPercent = 34;
    private int[] squareCheckNums = new int[8] { -9, -8, -7, -1, 1, 7, 8 , 9 };
    private int whiteNeighbors = 0;
    private bool entireGridBlack = false;

    private int[] multipleCheckNums1 = new int[] { 0,0,0,0,0,0,0,0,0,1,1,1,1,1,1,1,1,2,2,2,2,2,2,2,3,3,3,3,3,3,4,4,4,4,4,5,5,5,5,6,6,6,7,7,8 };
    private int[] multipleCheckNums2 = new int[] { 0,1,2,3,4,5,6,7,8,1,2,3,4,5,6,7,8,2,3,4,5,6,7,8,3,4,5,6,7,8,4,5,6,7,8,5,6,7,8,6,7,8,7,8,8 };

    private int[] secondGridOtherSolutions1 = new int[360];
    private int[] secondGridOtherSolutions2 = new int[360];
    private int secondGridWhichNum = 0;

    public Renderer[] squares;
    public Material[] squareMats;

    private int intPlaceholder = 0;
    private bool viewingGrid2 = false;
    private bool viewingGrid3 = false;
    public TextMesh bigButtonText;

    private string[] debugGrids = new string[3];
    private string debugInput;
    private string debugSolution;

    private int tpWhichColumn = 0;
    private bool tpNoLetter = false;

    // Use this for initialization
    void Start ()
    {
        initialization:
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
        for(int i = 0; i < 48; i++) //generate board
        {
            intPlaceholder = Rnd.Range(0,48);
            if (intPlaceholder > blackPercent)
            {
                grids[0,gridNums[i]] = true;
                squares[i].material = squareMats[1];
                inputGrid[i] = true;
                debugGrids[0] = debugGrids[0] + "#";
            }
            else
            {
                grids[0,gridNums[i]] = false;
                squares[i].material = squareMats[0];
                inputGrid[i] = false;
                debugGrids[0] = debugGrids[0] + "O";
            }
        }
        for(int i = 0; i < 48; i++) //2nd generation
        {
            whiteNeighbors = 0;
            for(int j = 0; j < 8; j++)
            {
                if(grids[0,gridNums[i] + squareCheckNums[j]]) //if this square that we're checking is white
                {
                    whiteNeighbors++;
                }
            }
            if(grids[0,gridNums[i]]) //if this square is white
            {
                if(whiteNeighbors < whiteStillAliveRanges[0] || whiteNeighbors > whiteStillAliveRanges[1])
                {
                    grids[1,gridNums[i]] = false;
                    debugGrids[1] = debugGrids[1] + "O";
                }
                else
                {
                    grids[1,gridNums[i]] = true;
                    debugGrids[1] = debugGrids[1] + "#";
                }
            }
            else
            {
                if(whiteNeighbors == blackToAliveSquaresNum)
                {
                    grids[1,gridNums[i]] = true;
                    debugGrids[1] = debugGrids[1] + "#";
                }
                else
                {
                    grids[1,gridNums[i]] = false;
                    debugGrids[1] = debugGrids[1] + "O";
                }
            }
        }
        for (int i = 0; i < 48; i++) //3rd generation
        {
            whiteNeighbors = 0;
            for (int j = 0; j < 8; j++)
            {
                if (grids[1,gridNums[i] + squareCheckNums[j]]) //if this square that we're checking is white
                {
                    whiteNeighbors++;
                }
            }
            if (grids[1, gridNums[i]]) //if this square is white
            {
                if (whiteNeighbors < whiteStillAliveRanges[0] || whiteNeighbors > whiteStillAliveRanges[1])
                {
                    grids[2,gridNums[i]] = false;
                    debugGrids[2] = debugGrids[2] + "O";
                }
                else
                {
                    grids[2,gridNums[i]] = true;
                    debugGrids[2] = debugGrids[2] + "#";
                }
            }
            else
            {
                if (whiteNeighbors == blackToAliveSquaresNum)
                {
                    grids[2,gridNums[i]] = true;
                    debugGrids[2] = debugGrids[2] + "#";
                }
                else
                {
                    grids[2,gridNums[i]] = false;
                    debugGrids[2] = debugGrids[2] + "O";
                }
            }
        }
        entireGridBlack = true;
        for(int i = 0; i < 48; i++) //check if boards 1 and 2 or boards 2 and 3 are the same, or if any of the stages are entirely black for more interesting modules
        {
            if(grids[0,gridNums[i]] == false && entireGridBlack)
            {
                if(i == 47)
                {
                    goto initialization;
                }
            }
            else
            {
                entireGridBlack = false;
            }
            if(grids[0,gridNums[i]] == grids[1,gridNums[i]])
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
        secondGridWhichNum = 0;
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
                for(int i = 0; i < 48; i++)
                {
                    if(placeholderGrid[gridNums[i]] != grids[1, gridNums[i]])
                    {
                        i = 48;
                    }
                    else if(i == 47)
                    {
                        secondGridOtherSolutions1[secondGridWhichNum] = g;
                        secondGridOtherSolutions2[secondGridWhichNum] = h;
                        secondGridWhichNum++;
                    }
                }
            }
        }
        if (secondGridWhichNum > 0)
        {
            for (int k = 0; k < secondGridWhichNum; k++)
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
                    }
                }
            }
        }
        DebugMsg("Grid one:");
        DebugMsg("" + debugGrids[0][0] + debugGrids[0][1] + debugGrids[0][2] + debugGrids[0][3] + debugGrids[0][4] + debugGrids[0][5]);
        DebugMsg("" + debugGrids[0][6] + debugGrids[0][7] + debugGrids[0][8] + debugGrids[0][9] + debugGrids[0][10] + debugGrids[0][11]);
        DebugMsg("" + debugGrids[0][12] + debugGrids[0][13] + debugGrids[0][14] + debugGrids[0][15] + debugGrids[0][16] + debugGrids[0][17]);
        DebugMsg("" + debugGrids[0][18] + debugGrids[0][19] + debugGrids[0][20] + debugGrids[0][21] + debugGrids[0][22] + debugGrids[0][23]);
        DebugMsg("" + debugGrids[0][24] + debugGrids[0][25] + debugGrids[0][26] + debugGrids[0][27] + debugGrids[0][28] + debugGrids[0][29]);
        DebugMsg("" + debugGrids[0][30] + debugGrids[0][31] + debugGrids[0][32] + debugGrids[0][33] + debugGrids[0][34] + debugGrids[0][35]);
        DebugMsg("" + debugGrids[0][36] + debugGrids[0][37] + debugGrids[0][38] + debugGrids[0][39] + debugGrids[0][40] + debugGrids[0][41]);
        DebugMsg("" + debugGrids[0][42] + debugGrids[0][43] + debugGrids[0][44] + debugGrids[0][45] + debugGrids[0][46] + debugGrids[0][47]);
        DebugMsg("Grid two:");
        DebugMsg("" + debugGrids[1][0] + debugGrids[1][1] + debugGrids[1][2] + debugGrids[1][3] + debugGrids[1][4] + debugGrids[1][5]);
        DebugMsg("" + debugGrids[1][6] + debugGrids[1][7] + debugGrids[1][8] + debugGrids[1][9] + debugGrids[1][10] + debugGrids[1][11]);
        DebugMsg("" + debugGrids[1][12] + debugGrids[1][13] + debugGrids[1][14] + debugGrids[1][15] + debugGrids[1][16] + debugGrids[1][17]);
        DebugMsg("" + debugGrids[1][18] + debugGrids[1][19] + debugGrids[1][20] + debugGrids[1][21] + debugGrids[1][22] + debugGrids[1][23]);
        DebugMsg("" + debugGrids[1][24] + debugGrids[1][25] + debugGrids[1][26] + debugGrids[1][27] + debugGrids[1][28] + debugGrids[1][29]);
        DebugMsg("" + debugGrids[1][30] + debugGrids[1][31] + debugGrids[1][32] + debugGrids[1][33] + debugGrids[1][34] + debugGrids[1][35]);
        DebugMsg("" + debugGrids[1][36] + debugGrids[1][37] + debugGrids[1][38] + debugGrids[1][39] + debugGrids[1][40] + debugGrids[1][41]);
        DebugMsg("" + debugGrids[1][42] + debugGrids[1][43] + debugGrids[1][44] + debugGrids[1][45] + debugGrids[1][46] + debugGrids[1][47]);
        DebugMsg("Grid three:");
        DebugMsg("" + debugGrids[2][0] + debugGrids[2][1] + debugGrids[2][2] + debugGrids[2][3] + debugGrids[2][4] + debugGrids[2][5]);
        DebugMsg("" + debugGrids[2][6] + debugGrids[2][7] + debugGrids[2][8] + debugGrids[2][9] + debugGrids[2][10] + debugGrids[2][11]);
        DebugMsg("" + debugGrids[2][12] + debugGrids[2][13] + debugGrids[2][14] + debugGrids[2][15] + debugGrids[2][16] + debugGrids[2][17]);
        DebugMsg("" + debugGrids[2][18] + debugGrids[2][19] + debugGrids[2][20] + debugGrids[2][21] + debugGrids[2][22] + debugGrids[2][23]);
        DebugMsg("" + debugGrids[2][24] + debugGrids[2][25] + debugGrids[2][26] + debugGrids[2][27] + debugGrids[2][28] + debugGrids[2][29]);
        DebugMsg("" + debugGrids[2][30] + debugGrids[2][31] + debugGrids[2][32] + debugGrids[2][33] + debugGrids[2][34] + debugGrids[2][35]);
        DebugMsg("" + debugGrids[2][36] + debugGrids[2][37] + debugGrids[2][38] + debugGrids[2][39] + debugGrids[2][40] + debugGrids[2][41]);
        DebugMsg("" + debugGrids[2][42] + debugGrids[2][43] + debugGrids[2][44] + debugGrids[2][45] + debugGrids[2][46] + debugGrids[2][47]);

        DebugMsg("Calculated solutions: " + secondGridWhichNum);
        DebugMsg("Intended number of neighbouring squares to turn a black square white: " + blackToAliveSquaresNum);
        DebugMsg("Intended range of neighbouring squares to keep a white square white: " + whiteStillAliveRanges[0] + " is the minimum number of squares, " + whiteStillAliveRanges[1] + " is the maximum number of squares.");
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
            debugInput = "";
            debugSolution = "";
            for(int i = 0; i < 48; i++)
            {
                if(inputGrid[i])
                {
                    debugInput = debugInput + "#";
                }
                else
                {
                    debugInput = debugInput + "O";
                }
            }
            DebugMsg("Inputted:");
            DebugMsg("" + debugInput[0] + debugInput[1] + debugInput[2] + debugInput[3] + debugInput[4] + debugInput[5]);
            DebugMsg("" + debugInput[6] + debugInput[7] + debugInput[8] + debugInput[9] + debugInput[10] + debugInput[11]);
            DebugMsg("" + debugInput[12] + debugInput[13] + debugInput[14] + debugInput[15] + debugInput[16] + debugInput[17]);
            DebugMsg("" + debugInput[18] + debugInput[19] + debugInput[20] + debugInput[21] + debugInput[22] + debugInput[23]);
            DebugMsg("" + debugInput[24] + debugInput[25] + debugInput[26] + debugInput[27] + debugInput[28] + debugInput[29]);
            DebugMsg("" + debugInput[30] + debugInput[31] + debugInput[32] + debugInput[33] + debugInput[34] + debugInput[35]);
            DebugMsg("" + debugInput[36] + debugInput[37] + debugInput[38] + debugInput[39] + debugInput[40] + debugInput[41]);
            DebugMsg("" + debugInput[42] + debugInput[43] + debugInput[44] + debugInput[45] + debugInput[46] + debugInput[47]);
            for (int i = 0; i < (secondGridWhichNum - 1); i++)
            {
                for(int j = 0; j < 48; j++)
                {
                    for (int k = 0; k < 48; k++)
                    {
                        if (inputGrid[k])
                        {
                            debugSolution = debugSolution + "#";
                        }
                        else
                        {
                            debugSolution = debugSolution + "O";
                        }
                    }
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

    void DebugMsg(string msg)
    {
        Debug.LogFormat("[Wack Game of Life #{0}] {1}", ModuleId, msg);
    }
}
