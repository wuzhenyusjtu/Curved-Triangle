// NX 10.0.0.24
// Journal created by rb2rru on Sun Mar 22 13:46:03 2015 China Standard Time
//
using System;
using NXOpen;

public class NXJournal
{
  public static void Main(string[] args)
  {
    NXOpen.Session theSession = NXOpen.Session.GetSession();
    NXOpen.Part workPart = theSession.Parts.Work;
    NXOpen.Part displayPart = theSession.Parts.Display;
    // ----------------------------------------------
    //   Menu: File->Open...
    // ----------------------------------------------
    // ----------------------------------------------
    //   Menu: File->Save
    // ----------------------------------------------
    NXOpen.Session.UndoMarkId markId1;
    markId1 = theSession.SetUndoMark(NXOpen.Session.MarkVisibility.Visible, "Start");
    
    theSession.SetUndoMarkName(markId1, "Name Parts Dialog");
    
    NXOpen.Session.UndoMarkId markId2;
    markId2 = theSession.SetUndoMark(NXOpen.Session.MarkVisibility.Invisible, "Name Parts");
    
    theSession.DeleteUndoMark(markId2, null);
    
    NXOpen.Session.UndoMarkId markId3;
    markId3 = theSession.SetUndoMark(NXOpen.Session.MarkVisibility.Invisible, "Name Parts");
    
    workPart.AssignPermanentName("C:\\Users\\rb2rru\\Desktop\\model1.prt");
    
    theSession.DeleteUndoMark(markId3, null);
    
    theSession.SetUndoMarkName(markId1, "Name Parts");
    
    NXOpen.PartSaveStatus partSaveStatus1;
    partSaveStatus1 = workPart.Save(NXOpen.BasePart.SaveComponents.True, NXOpen.BasePart.CloseAfterSave.False);
    
    partSaveStatus1.Dispose();
    // ----------------------------------------------
    //   Menu: Tools->Journal->Stop Recording
    // ----------------------------------------------
    
  }
  public static int GetUnloadOption(string dummy) { return (int)NXOpen.Session.LibraryUnloadOption.Immediately; }
}
