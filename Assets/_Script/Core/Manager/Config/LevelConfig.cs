using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewLevel", menuName = "ScriptableObjects/LevelConfig")]
public class LevelConfig : ScriptableObject
{
    public int XSize;
    public int YSize;
    public List<BoardColumn> Column = new List<BoardColumn>();
    public List<Jelly> Palette = new List<Jelly>();
    public List<Goal> Goals = new List<Goal>();
    public List<JellyBoxConfig> Turns = new List<JellyBoxConfig>();

    [ContextMenu("Fill Level")]
    private void Fill()
    {
        while(XSize != Column.Count)
        {
            if (XSize > Column.Count)
            {
                Column.Add(new BoardColumn());
            }
            else if (XSize < Column.Count)
            {
                Column.RemoveAt(Column.Count - 1);
            }
        }

        for (int i = 0; i < XSize; i++)
        {
            while (YSize != Column[i].Boxes.Count)
            {
                if (YSize > Column[i].Boxes.Count)
                {
                    Column[i].Boxes.Add(null);
                }
                else if (YSize < Column[i].Boxes.Count)
                {
                    Column[i].Boxes.RemoveAt(Column[i].Boxes.Count - 1);
                }
            }
        }
    }

    [ContextMenu("Test Level")]
    private void Test()
    {
        if (XSize != Column.Count)
        {
            DebugManager.Instance.LogError($"{name}: broken level. Fill it first !!!");
            return;
        }
        for (int i = 0; i < XSize; i++)
        {
            if (YSize != Column[i].Boxes.Count)
            {
                DebugManager.Instance.LogError($"{name}: broken level. Fill it first !!!");
                return;
            }
        }
        for (int i = 0, length = Palette.Count; i < length; i++)
        {
            if (Palette[i] == null)
            {
                DebugManager.Instance.LogError($"{name}: broken palette !!!");
                return;
            }
        }
        if (Goals.Count == 0)
        {
            DebugManager.Instance.LogError($"{name}: no goal !!!");
            return;
        }
        if (Turns.Count == 0)
        {
            DebugManager.Instance.LogError($"{name}: no turns !!!");
            return;
        }
        for (int i = 0, length = Goals.Count, max = Palette.Count; i < length; i++)
        {
            if (Goals[i].Index >= max)
            {
                DebugManager.Instance.LogError($"{name}: goal {i}-th exceed palette. Cannot be completed !!!");
                return;
            }
        }

        //Fill();
        JellyBoxConfig left, right, top, bottom;
        JellyConfig jelly;
        bool jellyDead;
        for (int x = 0; x < XSize; x++)
        {
            for (int y = 0; y < YSize; y++)
            {
                if (Column[x].Boxes[y] == null || Column[x].Boxes[y].Jellies.Count == 0)
                {
                    continue;
                }

                for (int i = 0, length = Column[x].Boxes[y].Jellies.Count; i < length; i++)
                {
                    jelly = Column[x].Boxes[y].Jellies[i];
                    if (jelly.Index >= Palette.Count)
                    {
                        DebugManager.Instance.LogError($"{x}-th column, {y}-th row has jelly {i}-th out of Palette !!!");
                        continue;
                    }

                    jellyDead = false;

                    left = x > 0 ? Column[x - 1].Boxes[y]:null;
                    right = x < XSize - 1 ? Column[x + 1].Boxes[y] : null;
                    top = y < YSize - 1 ? Column[x].Boxes[y + 1] : null;
                    bottom= y > 0 ? Column[x].Boxes[y - 1] : null;

                    if (left != null)
                    {
                        for (int ii = 0, lengthi = left.Jellies.Count; ii < lengthi; ii++)
                        {
                            if (Jelly.MatchJellyInRow(left.Jellies[ii].Layout, jelly.Layout, left.Jellies[ii].Index, jelly.Index))
                            {
                                jellyDead = true;
                                break;
                            }
                        }
                    }
                    if (!jellyDead && right != null)
                    {
                        for (int ii = 0, lengthi = right.Jellies.Count; ii < lengthi; ii++)
                        {
                            if (Jelly.MatchJellyInRow(jelly.Layout, right.Jellies[ii].Layout, jelly.Index, right.Jellies[ii].Index))
                            {
                                jellyDead = true;
                                break;
                            }
                        }
                    }
                    if (!jellyDead && top != null)
                    {
                        for (int ii = 0, lengthi = top.Jellies.Count; ii < lengthi; ii++)
                        {
                            if (Jelly.MatchJellyInColumn(top.Jellies[ii].Layout, jelly.Layout, top.Jellies[ii].Index, jelly.Index))
                            {
                                jellyDead = true;
                                break;
                            }
                        }
                    }
                    if (!jellyDead && bottom != null)
                    {
                        for (int ii = 0, lengthi = bottom.Jellies.Count; ii < lengthi; ii++)
                        {
                            if (Jelly.MatchJellyInColumn(jelly.Layout, bottom.Jellies[ii].Layout, jelly.Index, bottom.Jellies[ii].Index))
                            {
                                jellyDead = true;
                                break;
                            }
                        }
                    }

                    if (jellyDead)
                    {
                        DebugManager.Instance.LogError($"{x}-th column, {y}-th row has dead jellies at {i}");
                    }
                }
            }
        }
        DebugManager.Instance.Log($"{name}: good level.");
    }
}

[Serializable]
public class BoardColumn
{
    public List<JellyBoxConfig> Boxes = new List<JellyBoxConfig>();
}

[Serializable]
public class Goal
{
    public int Index;
    public int Count;
}