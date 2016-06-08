using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Asteroids
{
    public class UserControlledSprite : Sprite
    {
        protected float rotation = MathHelper.PiOver2;  // face right on startup
               
        public Vector2 Position
        {
            get { return position; }
        }

       // add to allow game to shoot missiles
       public float Rotation
        {
            get { return rotation; }
        }
        
        // Get direction of sprite movement - assumes texture points up
        public override Vector2 Direction
        {
            get
            {     
                return Vector2.Transform(-Vector2.UnitY, Matrix.CreateRotationZ(rotation));
            }
        }

        public UserControlledSprite(Texture2D textureImage, Vector2 position,
            int collisionOffset, float speed, Vector2 offset, float scale)
            : base(textureImage, position, collisionOffset,speed,offset, scale)
        {
        }


        public override void Update(GameTime gameTime, Rectangle clientBounds)
        {
            if (Keyboard.GetState().IsKeyDown(Keys.Right))
                rotation += 0.05f;
            if (Keyboard.GetState().IsKeyDown(Keys.Left))
                rotation -= 0.05f;

            position += Direction * speed;
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(textureImage, position, null, Color.WhiteSmoke, rotation, offset, scale, SpriteEffects.None, 1);
        }    

    }
}
