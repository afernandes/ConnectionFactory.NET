using System;
using System.Runtime.Serialization;

namespace ConnectionFactory
{
   /// <summary>
   /// This class represents the main exception for the Connection Factory.
   /// </summary>
   [Serializable]
   public class CfException : ApplicationException
   {
      /// <summary>
      /// Default Constructor.
      /// </summary>
      public CfException()
         : base()
      {
      }

      /// <summary>
      /// Constructor holding the error message.
      /// </summary>
      /// <param name="msg">Error message</param>
      public CfException(String msg)
         : base(msg)
      {
      }

      /// <summary>
      /// Constructor holding the error message and the thrown ineer exception.
      /// </summary>
      /// <param name="msg">Error message</param>
      /// <param name="innerExc">Thrown inner exception</param>
      public CfException(String msg, System.Exception innerExc)
         : base(msg, innerExc)
      {
      }

      /// <summary>
      /// Constructor used when serialization is needed.
      /// </summary>
      /// <param name="info">Serialization information</param>
      /// <param name="context">Serialization Streaming Context</param>
      protected CfException(SerializationInfo info, StreamingContext context)
         : base(info, context)
      {
      }
   }
}
