using EngineName.Components.Renderable;
using EngineName.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using EngineName.Components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace EngineName.Systems
{
    public class RenderingSystem : EcsSystem {
        private float bump = 0f;
        private GraphicsDevice mGraphicsDevice;
        private Vector3 lightDir = new Vector3(1f, -1f, 0.0f);
        public override void Init() {
            mGraphicsDevice = Game1.Inst.GraphicsDevice;
            base.Init();
        }
        public override void Update(float t, float dt) {
            if (Keyboard.GetState().IsKeyDown(Keys.T))
                bump += 0.01f;
            if(Keyboard.GetState().IsKeyDown(Keys.G))
                bump -= 0.01f;
            base.Update(t, dt);
        }
        public override void Draw(float t, float dt) {
            base.Draw(t, dt);

            Game1.Inst.GraphicsDevice.DepthStencilState = DepthStencilState.Default;


            foreach (CCamera camera in Game1.Inst.Scene.GetComponents<CCamera>().Values) {
                DrawScene(camera);
            }
            // debugging for software culling
            //Console.WriteLine(string.Format("{0} meshes drawn", counter));
        }

        public void DrawScene(CCamera camera, int excludeEid=-1) {
            // TODO: Clean code below up, hard to read.

            foreach (CTransform transformComponent in Game1.Inst.Scene.GetComponents<CTransform>().Values)
            {
                transformComponent.Frame = Matrix.CreateScale(transformComponent.Scale) *
                    transformComponent.Rotation *
                    Matrix.CreateTranslation(transformComponent.Position);
            }

            int counter = 0;
            foreach (var component in Game1.Inst.Scene.GetComponents<C3DRenderable>()) {
                var key = component.Key;

                if (key == excludeEid) {
                    // TODO: This is originally a hack to simplify rendering of environment maps.
                    continue;
                }

                C3DRenderable model = (C3DRenderable)component.Value;
                if (model.model == null) continue; // TODO: <- Should be an error, not silent fail?
                CTransform transform = (CTransform)Game1.Inst.Scene.GetComponentFromEntity<CTransform>(key);

                foreach (var mesh in model.model.Meshes)
                {

                    if (camera.Frustum.Contains(mesh.BoundingSphere.Transform(transform.Frame)) == ContainmentType.Disjoint)
                        continue;

                    // TODO: This might bug out with multiple mesh parts.
                    if (model.material != null) {
                        model.material.Model = mesh.ParentBone.Transform * transform.Frame;
                        model.material.View  = camera.View;
                        model.material.Proj  = camera.Projection;
                        model.material.Prerender();
                        model.material.mEffect.Parameters["BumpPower"].SetValue(bump);
                        model.material.mEffect.Parameters["FogColor"].SetValue(new Vector4(0.4f, 0.7f, 1.0f, 1.0f));
                        model.material.mEffect.Parameters["FogStart"].SetValue(20.0f);
                        model.material.mEffect.Parameters["FogEnd"].SetValue(200.0f);
                        model.material.mEffect.Parameters["LightDir"].SetValue(lightDir);
                        var device = Game1.Inst.GraphicsDevice;

                        for (var i = 0; i < mesh.MeshParts.Count; i++) {
                            var part = mesh.MeshParts[i];
                            var effect = model.material.mEffect;

                            device.SetVertexBuffer(part.VertexBuffer);
                            device.Indices = part.IndexBuffer;

                            for (var j = 0; j < effect.CurrentTechnique.Passes.Count; j++) {
                                effect.CurrentTechnique.Passes[j].Apply();
                                device.DrawIndexedPrimitives(PrimitiveType.TriangleList,
                                                             part.VertexOffset,
                                                             0,
                                                             part.NumVertices,
                                                             part.StartIndex,
                                                             part.PrimitiveCount);
                            }
                        }
                    }
                    else {
                        foreach (BasicEffect effect in mesh.Effects) {
                            //effect.EnableDefaultLighting();
                            effect.LightingEnabled = true;
                            effect.DirectionalLight0.Enabled = true;
                            //effect.AmbientLightColor = new Vector3(1f, 1f, 1f);
                            
                            effect.DirectionalLight0.DiffuseColor = new Vector3(0.3f, 0.3f, 0.3f);
                            effect.DirectionalLight0.Direction = lightDir;
                            //effect.SpecularColor = new Vector3(1, 1, 1);
                            effect.SpecularPower = 100;
                            effect.DirectionalLight0.SpecularColor = new Vector3(1f, 1f, 1f);
                            effect.PreferPerPixelLighting = true;
                            effect.FogEnabled = true;
                            effect.FogStart = 20.0f;
                            effect.FogEnd = 200.0f;
                            effect.FogColor = new Vector3(0.4f, 0.7f, 1.0f);
                            effect.Projection = camera.Projection;
                            effect.View = camera.View;
                            effect.World = mesh.ParentBone.Transform * transform.Frame;
                        }

                        mesh.Draw();
                    }
                }
            }
        }
    }
}
