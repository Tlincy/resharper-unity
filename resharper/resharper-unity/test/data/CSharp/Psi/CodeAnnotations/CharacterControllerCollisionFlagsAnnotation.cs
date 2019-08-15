using UnityEngine;

public class Class
{
  public void Method(CharacterController controller)
  {
    // Unity doesn't mark CollisionFlags with [Flags], so the & operator is marked with
    // "Bitwise operation on enum not marked by [Flags] annotation"
    if ((controller.collisionFlags & CollisionFlags.Above) != 0)
    {
    }
  }
}
