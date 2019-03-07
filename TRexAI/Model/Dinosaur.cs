using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TRexAI
{
    public class Dinosaur : GameObject
    {
        public bool isTouchingGround;
        
        public Dinosaur(GameObject gameObject) : base(gameObject.position.x, gameObject.position.y)
        {
            position = gameObject.position;
            size = gameObject.size;
            id = gameObject.id;
            objectType = ObjectType.Dinosaur;
        }

        public Dinosaur(int positionX, int positionY) : base(positionX, positionY)
        {
            objectType = ObjectType.Dinosaur;
        }
    }

}
