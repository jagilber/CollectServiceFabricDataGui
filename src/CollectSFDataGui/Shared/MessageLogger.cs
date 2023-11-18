using System;
using System.Collections.Generic;

namespace CollectSFDataGui.Shared
{
  public static class MessageLogger
  {
    public static List<string> Messages;

    static MessageLogger()
    {
      Messages = new List<string>();

    }
  }
}