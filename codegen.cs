using System; 

namespace CC9
{
    public partial class Program
    {
        // 抽象構文木を下りながらコード生成
        /// <summary>コード生成</summary>
        /// <param name="node">ノード</param>
        static void gen(Node node) {
            switch (node.kind) {
            case NodeKind.ND_NUM:
                Console.Write($"  push {node.val}\n");
                return;
            case NodeKind.ND_LVAR:
                gen_lval(node);
                Console.Write("  pop rax\n");
                Console.Write("  mov rax, [rax]\n");
                Console.Write("  push rax\n");
                return;
            case NodeKind.ND_ASSIGN:
                gen_lval(node.lhs);
                gen(node.rhs);

                Console.Write("  pop rdi\n");
                Console.Write("  pop rax\n");
                Console.Write("  mov [rax], rdi\n");
                Console.Write("  push rdi\n");
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

        /// <summary>ローカル変数のコード生成</summary>
        /// <param name="node">ノード</param>
        static void gen_lval(Node node) {
            if (node.kind != NodeKind.ND_LVAR){
               Console.Write("代入の左辺値が変数ではありません\n");
            }
            Console.Write("  mov rax, rbp\n");
            Console.Write($"  sub rax, {node.offset}\n");
            Console.Write("  push rax\n");
        }
    }
}