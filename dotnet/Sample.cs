namespace EdgeReference.DotNetTest
{
  public static class Type1Proxy
  {

    public static int CreateType2(
      int referenceId,
      int template,
      string description) 
    {
      Type2 _reference = ReferenceManager.Instance.PullReference(referenceId);
      Type2 _template = ReferenceManager.Instance.PullReference(template);

      Type2 _result = _reference(_template, description);
      int convertedResult = EnsureReference(_result);

      return convertedResult;
    }

  }
}
