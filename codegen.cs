using System; 

namespace CC9
{
        public partial class Program
    {
        // 抽象構文木を下りながらコード生成
        /// <summary>コード生成</summary>
        /// <param name="node">ノード</param>
        static void gen(Node node) {
            if (node.kind == NodeKind.ND_NUM) {
                Console.Write($"  push {node.val}\n");
                return;
            }

            gen(node.lhs);
            gen(node.rhs);

            Console.Write("  pop rdi\n");
            Console.Write("  pop rax\n");

            switch (node.kind) {
            case NodeKind.ND_ADD:
                Console.Write("  add rax, rdi\n");
                break;
            case NodeKind.ND_SUB:
                Console.Write("  sub rax, rdi\n");
                break;
            case NodeKind.ND_MUL:
                Console.Write("  imul rax, rdi\n");
                break;
            case NodeKind.ND_DIV:
                Console.Write("  cqo\n");
                Console.Write("  idiv rdi\n");
                break;
            case NodeKind.ND_EQ:
                Console.Write("  cmp rax, rdi\n");
                Console.Write("  sete al\n");
                Console.Write("  movzb rax, al\n");
                break;
            case NodeKind.ND_NE:
                Console.Write("  cmp rax, rdi\n");
                Console.Write("  setne al\n");
                Console.Write("  movzb rax, al\n");
                break;
            case NodeKind.ND_LT:
                Console.Write("  cmp rax, rdi\n");
                Console.Write("  setl al\n");
                Console.Write("  movzb rax, al\n");
                break;
            case NodeKind.ND_LE:
                Console.Write("  cmp rax, rdi\n");
                Console.Write("  setle al\n");
                Console.Write("  movzb rax, al\n");
                break;
            }

            Console.Write("  push rax\n");
        }
    }
}