using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Asteroids
{
    public class ProjectileSprite : Sprite
    {
        protected float rotation;
        ParticleEngine engine;
        Vector2 emitterLocation;

        public Vector2 Position
        {
            get { return position; }        
        }

        // Get direction of sprite movement - assumes texture points up
        public override Vector2 Direction
        {
            get
            {
                return Vector2.Transform(-Vector2.UnitY, Matrix.CreateRotationZ(rotation));
            }
        }

        public ProjectileSprite(Texture2D textureImage, Vector2 position,
            int collisionOffset, float speed, Vector2 offset, float scale, float rotation, List<Texture2D> textures)
            : base(textureImage, position, collisionOffset, speed, offset, scale)
        {
            this.rotation = rotation;
            engine = new ParticleEngine(textures, Vector2.Zero);
        }

        public override void Update(GameTime gameTime, Rectangle clientBounds)
        {
            position += Direction * speed;
            //position is the tip of the missile; find the end (emitter location) with a bit of math
            Vector2 end = new Vector2(
                    (float)(position.X - Math.Cos(rotation - Math.PI / 2) * textureImage.Height * scale),
                    (float)(position.Y - Math.Sin(rotation - Math.PI / 2) * textureImage.Height * scale));
            emitterLocation = end;
            engine.EmitterLocation = emitterLocation;
            engine.Update();
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.FrontToBack, BlendState.Additive);
            engine.Draw(spriteBatch);
            spriteBatch.End();
            spriteBatch.Begin();
            spriteBatch.Draw(textureImage, position, null, Color.Red, rotation, offset, scale, SpriteEffects.None, 1);
        }
    }
}
