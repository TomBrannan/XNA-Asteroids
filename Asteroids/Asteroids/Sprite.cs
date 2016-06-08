using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace Asteroids
{    
    public abstract class Sprite
    {
        // sprite's texture image
        protected Texture2D textureImage;

        // drawing info
        protected Vector2 offset;
        protected float scale;

        // Movement data
        protected float speed;
        protected Vector2 position;

        // number of pixels from each edge to ignore
        protected int collisionOffset;

        // Abstract definition of direction property
        public abstract Vector2 Direction
        {
            get;
        }

        // Constructor
        public Sprite(Texture2D textureImage, Vector2 position,
            int collisionOffset,  float speed, Vector2 offset, float scale)
        {
            this.textureImage = textureImage;
            this.position = position;
            this.collisionOffset = collisionOffset;
            this.speed = speed;
            this.offset = offset;
            this.scale = scale;
        }

        // Update method
        public virtual void Update(GameTime gameTime, Rectangle clientBounds)
        {           
        }

        // Draw method
        public virtual void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {        
        }

        // Gets the collision rect based on position, scale, offset and collision offset
        public virtual Rectangle CollisionRect
        {
            get
            {
                int w = textureImage.Width;
                int h = textureImage.Height;
                float s = 0.8f;

                if (textureImage.Height >= 2 * textureImage.Width)
                {
                    h = w;
                    return new Rectangle(
                    (int)(position.X + (collisionOffset - offset.X) * scale * s),
                    (int)(position.Y + (collisionOffset - 0) * scale * s),
                    (int)((w - collisionOffset * 2) * scale * s),
                    (int)((h - collisionOffset * 2) * scale * s));
                     
                }
                return new Rectangle(
                    (int)(position.X + (collisionOffset - offset.X) * scale * s),
                    (int)(position.Y + (collisionOffset - offset.Y) * scale * s),
                    (int)((w - collisionOffset * 2) * scale * s),
                    (int)((h - collisionOffset * 2) * scale * s));
            }
        }
        
    }
}
