using System.Collections.Generic;

public class PanelManager : MonoSingleton<PanelManager>
{
    private List<AbsPanel> _showStack = new List<AbsPanel>();

    public void CheckShowStackOnShow(AbsPanel absPanel)
    {
        AbsPanel topPanel = _showStack.Count > 0 ? _showStack[_showStack.Count - 1] : null;
        _showStack.Remove(absPanel);
        _showStack.Add(absPanel);
        if (absPanel == topPanel || topPanel == null) return;
        topPanel.Hide();
    }

    public void CheckShowStackOnHide(AbsPanel absPanel)
    {
        AbsPanel topPanel = _showStack.Count > 0 ? _showStack[_showStack.Count - 1] : null;
        AbsPanel nextPanel = _showStack.Count > 1 ? _showStack[_showStack.Count - 2] : null;
        _showStack.Remove(absPanel);
        if (absPanel != topPanel || nextPanel == null) return;
        nextPanel.Show();
    }
}
