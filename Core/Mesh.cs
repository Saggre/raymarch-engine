using EconSim.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EconSim.Core
{
    public class Mesh
    {
        private RenderVertex[] vertices;
        public Mesh(RenderVertex[] vertices)
        {
            this.vertices = vertices;
        }

        public RenderVertex[] Vertices
        {
            get => vertices;
            set => vertices = value;
        }
    }
}
