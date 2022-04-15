using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TiltBrush
{
  public class BrushList : MonoBehaviour
  {
    public Transform container;
    public BrushItem brushItemPrefab;

    public List<BrushItem> items = new List<BrushItem>();

    private List<BrushDescriptor> allBrushes;
    private int currPage = 0;
    private int maxPage => Mathf.CeilToInt(allBrushes.Count / items.Count);
    private BrushDescriptor selectedBrush;

    void Awake()
    {
      items = container.GetComponentsInChildren<BrushItem>().ToList();
    }

    void Start()
    {
      allBrushes = BrushCatalog.m_Instance.AllBrushes.ToList();

      UpdatePage();

      selectedBrush = BrushCatalog.m_Instance.DefaultBrush;
      BrushController.m_Instance.BrushChanged += UpdateSelectedBrush;

      UpdateSelectedBrush(selectedBrush);
    }

    public void UpdateSelectedBrush(BrushDescriptor newBrush)
    {
      selectedBrush = newBrush;
      foreach (var item in items)
      {
        if(item.gameObject.activeSelf)
          item.SetSelected(item && item.Brush.UniqueName == newBrush.UniqueName);
      }
    }

    public void NextPage()
    {
      if (currPage < maxPage)
      {
        currPage += 1;
        UpdatePage();
        UpdateSelectedBrush(selectedBrush);
      }
    }

    public void PreviousPage()
    {
      if (currPage > 0)
      {
        currPage -= 1;
        UpdatePage();
        UpdateSelectedBrush(selectedBrush);
      }
    }

    private void UpdatePage()
    {
      var startIndex = currPage * items.Count;
      for (var i = 0; i < items.Count; i++)
      {
        if (startIndex + i < allBrushes.Count)
        {
          items[i].gameObject.SetActive(true);
          items[i].SetInfo(allBrushes[startIndex + i]);
        }
        else
        {
          items[i].SetInfo(null);
          items[i].gameObject.SetActive(false);
        }
      }
    }
  }

}