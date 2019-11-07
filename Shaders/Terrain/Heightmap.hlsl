RWStructuredBuffer<float> Result;

[numthreads(32, 32, 1)]
void main(uint3 id : SV_DispatchThreadID)
{
    //valori per riga
    int stride = 1024;
	//indice linearizzato
    int idx = id.y * stride + id.x;

    Result[idx] = 1;
}
