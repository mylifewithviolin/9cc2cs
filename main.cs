using System; 


namespace CC9
{
        /// <summary>プログラム型</summary>
    public partial class Program
    {
        /// <summary>メイン</summary>
        /// <param name="args">引数</param>
        static int Main(string[] args)
        {
              if (args.Length != 2) {
                    Console.Write( "引数の個数が正しくありません\n");
                    return 1;
                }

            //Nodeリストを初期化する
            List<Node> codeList = new List<Node>();
            // トークナイズする
            List<Token> tokenList = tokenize(args[1]);
            // 現在着目しているトークン
            int curIndex=0;
            // パースする
            program(codeList,tokenList,ref curIndex);

            // アセンブリの前半部分を出力
            Console.Write(".intel_syntax noprefix\n");
            Console.Write(".globl main\n");
            Console.Write("main:\n");

            // プロローグ
            // 変数26個分の領域を確保する
            Console.Write("  push rbp\n");
            Console.Write("  mov rbp, rsp\n");
            Console.Write("  sub rsp, 208\n");

            // 先頭の式から順にコード生成
            for (int i = 0; i< codeList.Count; i++) {

                gen(codeList.ElementAt(i));//コード;

                // 式の評価結果としてスタックに一つの値が残っている
                // はずなので、スタックが溢れないようにポップしておく
                Console.Write("  pop rax\n");
            }

            // エピローグ
            // 最後の式の結果がRAXに残っているのでそれが返り値になる
            Console.Write("  mov rsp, rbp\n");
            Console.Write("  pop rbp\n");         
            Console.Write("  ret\n");
            return 0;
        }
        
     
    }
}