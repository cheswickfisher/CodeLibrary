using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Ross.Decals
{
    public class PackedRenderer
    {
        public Renderer r;
        public Material material;
        //public RenderTexture uvIslands;
        public RenderTexture source;
        public RenderTexture target;

        public PackedRenderer(Renderer r, Material material, RenderTexture rt)
        {
            this.r = r;
            this.material = material;
           // this.uvIslands = rt;
            source = new RenderTexture(rt);
            target = new RenderTexture(rt);
        }
    }

    public class UVPainter : MonoBehaviour
    {
        Matrix4x4 projection;
        Matrix4x4 view;

        private Material projectionMaterial;
        public Shader projectionShader;
        public Material paintMaterial;
        private Material uvDilationMat;
        public Shader uvDilationShader;
        private Material markUVIslandsMat;
        public Shader markUVIslandsShader;
        public Texture2D _Decal;
        public Color decalColor;
        public Renderer r;

        RenderTexture source;
        RenderTexture target;
        RenderTexture blankTexture;

        CommandBuffer buffer;

        Projector projector;

        Dictionary<Renderer, PackedRenderer> renderers = new Dictionary<Renderer, PackedRenderer>();

        static int
           textureID = Shader.PropertyToID("_MainTex");

        void Start()
        {

            projectionMaterial = new Material(projectionShader);
            projectionMaterial.SetColor("_Color", decalColor);
            projectionMaterial.SetTexture("_Decal", _Decal);
            uvDilationMat = new Material(uvDilationShader);
            markUVIslandsMat = new Material(markUVIslandsShader);

            projector = new Projector(Vector3.zero, Quaternion.identity, Vector2.one, .3f, projectionMaterial);

            target = new RenderTexture(1024, 1024, 0);
            target.name = "target projector tex";
            source = new RenderTexture(1024, 1024, 0);
            source.name = "source projector tex";
            buffer = new CommandBuffer();
            blankTexture = new RenderTexture(1024, 1024, 0);
            blankTexture.name = "blankTexure";

            Graphics.SetRenderTarget(blankTexture);
            GL.Clear(false, true, Color.clear);
            Graphics.SetRenderTarget(source);
            GL.Clear(false, true, Color.clear);
            Graphics.SetRenderTarget(target);
            GL.Clear(false, true, Color.clear);
        }

        void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                Paint();
            }
        }

        private RenderTexture MarkIslands(Renderer r)
        {
            markUVIslandsMat.SetMatrix("mesh_Object2World", r.localToWorldMatrix);
            RenderTexture source = new RenderTexture(blankTexture);
            RenderTexture target = new RenderTexture(blankTexture);

            CommandBuffer tempBuffer = new CommandBuffer();
            tempBuffer.SetRenderTarget(source);
            tempBuffer.DrawRenderer(r, markUVIslandsMat, 0);

            Graphics.ExecuteCommandBuffer(tempBuffer);

            return source;
        }



        private void RenderDecal(Renderer r)
        {

            if (!renderers.ContainsKey(r))
            {
                renderers.Add(r, new PackedRenderer(r, r.material, blankTexture));
            }

            PackedRenderer pr = renderers[r];          

            uvDilationMat.SetTexture(textureID, pr.target);
            pr.material.SetTexture("_MainTex", pr.source);
            projectionMaterial.SetColor("_Color", decalColor);

            buffer.Clear();

            buffer.SetRenderTarget(pr.target);

            buffer.SetViewProjectionMatrices(projector.View, projector.Projection);
            Vector2 pixelRect = new Vector2(target.width, target.height);
            buffer.SetViewport(new Rect(Vector2.zero, pixelRect));

            buffer.DrawRenderer(pr.r, projectionMaterial, 0);
            buffer.Blit(pr.target, pr.source, uvDilationMat);


            Graphics.ExecuteCommandBuffer(buffer);
        }

        void Paint()
        {

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            Vector3 hitPoint = Vector3.positiveInfinity;
            if (Physics.Raycast(ray, out hit, 100.0f, 1 << 4, QueryTriggerInteraction.Ignore))
            {
                hitPoint = hit.point;
                projector.position = hitPoint;
                projector.rotation = Quaternion.FromToRotation(Vector3.forward, -hit.normal);
                RenderDecal(hit.transform.root.GetComponentInChildren<Renderer>());
            }
        }

       /* private void OnDrawGizmos()
        {
            projector.DrawProjector(false);
        }

        private void OnDrawGizmosSelected()
        {
            projector.DrawProjector(true);
        }*/

    }
}
