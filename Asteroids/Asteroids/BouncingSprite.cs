using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Asteroids
{
    public class BouncingSprite : Sprite
    {
        Vector2 direction;
        float spin, spinRate;
        Rectangle bounds;
        public Vector2 Position
        {
            get { return position; }        
        }

        // Get direction of sprite movement - assumes texture points up
        public override Vector2 Direction
        {
            get
            {
                return direction;
            }
        }

        public BouncingSprite(Texture2D textureImage, Vector2 position,
            int collisionOffset, float speed, Vector2 offset, float scale, float spinRate, Vector2 
            direction, Rectangle bounds)
            : base(textureImage, position, collisionOffset, speed, offset, scale)
        {
            spin = 0;
            this.spinRate = spinRate;
            this.direction = direction;
            this.bounds = bounds;
        }

        public override void Update(GameTime gameTime, Rectangle clientBounds)
        {
            if (position.X <= 0 || position.X >= bounds.Width - offset.X)
            {
                direction.X *= -1f;
            }
            if (position.Y <= 0 || position.Y >= bounds.Height - offset.Y)
            {
                direction.Y *= -1f;
            }
            position += direction * speed;
            spin += spinRate;
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(textureImage, position, null, Color.WhiteSmoke, spin, offset, scale, SpriteEffects.None, 1);
        }
    }
}
