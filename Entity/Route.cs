namespace NobUS.DataContract.Model.Entity
{
  public sealed partial record Route(string Name, string Caption, Station[] Stations);
}
