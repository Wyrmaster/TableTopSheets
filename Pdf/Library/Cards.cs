using TableTopSheets.Pdf.Component.Generic;
using TableTopSheets.Pdf.Page;

namespace TableTopSheets.Pdf;

public class Cards: Document
{
  private readonly (string header, string body)[] _itemCards;

  #region Constructor

  public Cards(params (string header, string body)[] itemCards)
  {
    this._itemCards = itemCards;
  }

  #endregion
  
  #region Override
  
  protected override void Generate()
  {
    CardPage<ItemCard> cardPage = new CardPage<ItemCard>(this.PdfDocument, this._itemCards); 
    cardPage.Draw();
  }

  #endregion
}