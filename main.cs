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

            // トークナイズする
            List<Token> tokenList = tokenize(args[1]);
            // 現在着目しているトークン
            int curIndex=0;
            // パースする
            Node node = expr(tokenList,ref curIndex);

            // アセンブリの前半部分を出力
            Console.Write(".intel_syntax noprefix\n");
            Console.Write(".globl main\n");
            Console.Write("main:\n");

            // 抽象構文木を下りながらコード生成
            gen(node);

              // スタックトップに式全体の値が残っているはずなので
            // それをRAXにロードして関数からの返り値とする
            Console.Write("  pop rax\n");
           
            Console.Write("  ret\n");
            return 0;
        }
        
     
    }
}