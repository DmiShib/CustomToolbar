namespace CustomToolbar.Editor.ToolbarElements.Favorites.Data
{
      using System.Collections.Generic;


      [System.Serializable]
      public class FavoriteList
      {
            public string name = "New List";
            public List<FavoriteItem> items = new();
      }
}