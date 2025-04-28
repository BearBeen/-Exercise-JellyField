using System.Collections.Generic;

using UnityEngine;

public class JellyBox : MonoBehaviour, IPoolable
{
    List<Jelly> _jellies = new List<Jelly>();
    List<Jelly> _deadJellies = new List<Jelly>();
    public int jellyCount => _jellies.Count;

    public void InitJellies(List<Jelly> jellies)
    {
        _jellies = jellies;
        ExpandJellies(0);
        for (int i = 0, length = jellies.Count; i < length; i++)
        {
            jellies[i].transform.parent = transform;
            jellies[i].JellyFitInEffect(0, 0);
        }
    }

    //expand if need all the jelly
    public void ExpandJellies(float delay)
    {
        int surface = 0;
        for (int i = 0, length = _jellies.Count; i < length; i++)
        {
            Jelly.TryLayout(_jellies[i].layout, ref surface);
        }
        if (surface == Jelly.FULL_SURFACE)
        {
            return;
        }
        //expand left
        for (int i = 0, length = _jellies.Count; i < length; i++)
        {
            surface = _jellies[i].TryExpandLeft(surface, delay);
        }
        if (surface == Jelly.FULL_SURFACE)
        {
            return;
        }
        //expand right
        for (int i = 0, length = _jellies.Count; i < length; i++)
        {
            surface = _jellies[i].TryExpandRight(surface, delay);
        }
        if (surface == Jelly.FULL_SURFACE)
        {
            return;
        }
        //expand top
        for (int i = 0, length = _jellies.Count; i < length; i++)
        {
            surface = _jellies[i].TryExpandTop(surface, delay);
        }
        if (surface == Jelly.FULL_SURFACE)
        {
            return;
        }
        //expand bottom
        for (int i = 0, length = _jellies.Count; i < length; i++)
        {
            surface = _jellies[i].TryExpandBottom(surface, delay);
        }
    }

    //matching with nearby box to check if i have to delete any of my jellies
    public bool MatchJellyBox(JellyBox left, JellyBox right, JellyBox top, JellyBox bottom)
    {
        Jelly jelly;
        bool jellyDead;
        for (int i = 0, length = _jellies.Count; i < length; i++)
        {
            jelly = _jellies[i];
            jellyDead = false;
            if (left != null)
            {
                for (int ii = 0, lengthi = left._jellies.Count; ii < lengthi; ii++)
                {
                    if (Jelly.MatchJellyInRow(left._jellies[ii].layout, jelly.layout, left._jellies[ii].index, jelly.index))
                    {
                        _deadJellies.Add(jelly);
                        jelly.Die();
                        jellyDead = true;
                        break;
                    }
                }
            }
            if (!jellyDead && right != null)
            {
                for (int ii = 0, lengthi = right._jellies.Count; ii < lengthi; ii++)
                {
                    if (Jelly.MatchJellyInRow(jelly.layout, right._jellies[ii].layout, jelly.index, right._jellies[ii].index))
                    {
                        _deadJellies.Add(jelly);
                        jelly.Die();
                        jellyDead = true;
                        break;
                    }
                }
            }
            if (!jellyDead && top != null)
            {
                for (int ii = 0, lengthi = top._jellies.Count; ii < lengthi; ii++)
                {
                    if (Jelly.MatchJellyInColumn(top._jellies[ii].layout, jelly.layout, top._jellies[ii].index, jelly.index))
                    {
                        _deadJellies.Add(jelly);
                        jelly.Die();
                        jellyDead = true;
                        break;
                    }
                }
            }
            if (!jellyDead && bottom != null)
            {
                for (int ii = 0, lengthi = bottom._jellies.Count; ii < lengthi; ii++)
                {
                    if (Jelly.MatchJellyInColumn(jelly.layout, bottom._jellies[ii].layout, jelly.index, bottom._jellies[ii].index))
                    {
                        _deadJellies.Add(jelly);
                        jelly.Die();
                        jellyDead = true;
                        break;
                    }
                }
            }
        }

        //destroy _deadJellies jelly
        if (_deadJellies.Count > 0)
        {
            return true;
        }
        return false;
    }

    public void ClearDead()
    {
        for (int i = 0, length = _deadJellies.Count; i < length; i++)
        {
            _jellies.Remove(_deadJellies[i]);
        }
        _deadJellies.Clear();
    }

    public void FindJelly(int paletteIndex, ref List<Jelly> result)
    {
        for (int i = 0; i < _jellies.Count; i++)
        {
            Jelly jelly = _jellies[i];
            if (jelly.index == paletteIndex)
            {
                result.Add(jelly);
            }
        }
    }

    public void KillJelly(int paletteIndex)
    {
        bool isJellyDead = false;
        for (int i = 0; i < _jellies.Count; i++)
        {
            Jelly jelly = _jellies[i];
            if (jelly.index == paletteIndex || paletteIndex < 0)
            {
                isJellyDead = true;
                _jellies.RemoveAt(i);
                jelly.Die();
                i--;
            }
        }
        if (isJellyDead)
        {
            ExpandJellies(0);
        }
    }

    public void Destroy()
    {
        Destroy(gameObject);
    }

    public void OnReturnToPool()
    {

        for (int i = 0, length = _jellies.Count; i < length; i++)
        {
            _jellies[i].Recycle();
        }
        transform.parent = GameBoardManager.Instance.poolRoot;
    }

    public void OnGetFomPool()
    {
        //throw new System.NotImplementedException();
    }

    public void Recycle()
    {
        GameBoardManager.Instance.RecycleBox(this);
    }

    public static List<int> CountJellyIndices(JellyBox left, JellyBox right, JellyBox top, JellyBox bottom)
    {
        return Jelly.CountJellyIndices(left?._jellies, right?._jellies, top?._jellies, bottom?._jellies);
    }
}