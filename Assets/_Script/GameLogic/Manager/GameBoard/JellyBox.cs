using System.Collections.Generic;
using UnityEngine;

public class JellyPalette : ScriptableObject
{
    public List<Jelly> Jellies = new List<Jelly>();
}

public class JellyBox: MonoBehaviour, IPoolable
{
    List<Jelly> _jellies = new List<Jelly>();
    List<Jelly> _deadJellies = new List<Jelly>();
    public int jellyCount => _jellies.Count;
    //public List<Jelly> jellies => _jellies;

    public void InitJellies(List<Jelly> jellies)
    {
        _jellies = jellies;
        ExpandJellies();
        for (int i = 0, length = jellies.Count; i < length; i++)
        {
            jellies[i].transform.parent = transform;
            jellies[i].RenderJelly(0);
        }
    }

    //expand if need all the jelly
    public void ExpandJellies()
    {
        int surface = 0;
        List<Jelly> alive = new List<Jelly>();
        for (int i = 0, length = _jellies.Count; i < length; i++)
        {
            if (!_deadJellies.Contains( _jellies[i]))
            {
                alive.Add(_jellies[i]);
            }
        }
        for (int i = 0, length = alive.Count; i < length; i++)
        {
            Jelly.TryLayout(alive[i].layout, ref surface);
        }
        if (surface == Jelly.FULL_SURFACE)
        {
            return;
        }
        //expand left
        for (int i = 0, length = alive.Count; i < length; i++)
        {
            surface = alive[i].TryExpandLeft(surface);
        }
        if (surface == Jelly.FULL_SURFACE)
        {
            return;
        }
        //expand right
        for (int i = 0, length = alive.Count; i < length; i++)
        {
            surface = alive[i].TryExpandRight(surface);
        }
        if (surface == Jelly.FULL_SURFACE)
        {
            return;
        }
        //expand top
        for (int i = 0, length = alive.Count; i < length; i++)
        {
            surface = alive[i].TryExpandTop(surface);
        }
        if (surface == Jelly.FULL_SURFACE)
        {
            return;
        }
        //expand bottom
        for (int i = 0, length = alive.Count; i < length; i++)
        {
            surface = alive[i].TryExpandBottom(surface);
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
                        jelly.DoDead();
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
                        jelly.DoDead();
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
                        jelly.DoDead();
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
                        jelly.DoDead();
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
            _deadJellies[i].Recycle();
        }
        _deadJellies.Clear();
    }

    public void Destroy()
    {
        Destroy(gameObject);
    }

    public void OnDisabled()
    {
        
        for (int i = 0, length = _jellies.Count; i < length; i++)
        {
            _jellies[i].Recycle();
        }
        transform.parent = GameBoardManager.Instance.poolRoot;
    }

    public void OnEnabled()
    {
        //throw new System.NotImplementedException();
    }

    public void Recycle()
    {
        GameBoardManager.Instance.RecycleBox(this);
    }
}