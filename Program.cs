using System; 
using System.Text.RegularExpressions;
using System.Linq;

namespace CC9
{
    class Program
    {
        const string TOKEN_SYNBL_PLUS ="+";
        const string TOKEN_SYNBL_MINUS ="-";
        // トークンの種類
        enum TokenKind{
            TK_RESERVED, // 記号
            TK_NUM,      // 整数トークン
            TK_EOF,      // 入力の終わりを表すトークン
        } ;

        // トークン型
        // struct Token {
        //     TokenKind kind; // トークンの型
        //     Token *next;    // 次の入力トークン
        //     int val;        // kindがTK_NUMの場合、その数値
        //     char *str;      // トークン文字列
        // };

        // 抽象構文木のノードの種類
        enum NodeKind{
            ND_ADD, // +
            ND_SUB, // -
            ND_MUL, // *
            ND_DIV, // /
            ND_NUM, // 整数
        } ;


        class Token {
            public TokenKind kind; // トークンの型
            public  int next;    // 次の入力トークン
            public int val;        // kindがTK_NUMの場合、その数値
            public string? str;      // トークン文字列
        };

        // 現在着目しているトークン
        //private Token token;
        

        // 抽象構文木のノードの型
        class  Node {
            public NodeKind kind; // ノードの型
            public Node? lhs;     // 左辺
            public Node? rhs;     // 右辺
            public int val;       // kindがND_NUMの場合のみ使う
        };

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

            // 式の最初は数でなければならないので、それをチェックして
            // 最初のmov命令を出力
            //printf("  mov rax, %d\n", expect_number());
            //Console.Write($"  mov rax, {expect_number(token,ref curIndex)}\n" );

            // `+ <数>`あるいは`- <数>`というトークンの並びを消費しつつ
            // アセンブリを出力
            // while (curIndex<tokenList.Count) {
            //     //curIndex= token.next;
            //     token = tokenList.ElementAt(curIndex);//次のトークン
            //     if(at_eof(token))break;
                
            //     if (consume(token,"+",ref curIndex)) {
            //         token = tokenList.ElementAt(curIndex);//次のトークン
            //         //printf("  add rax, %d\n", expect_number());
            //         Console.Write($"  mov rax, {expect_number(token,ref curIndex)}\n" );
            //         continue;
            //     }

            //     expect(token,"-",ref curIndex);
            //     token = tokenList.ElementAt(curIndex);//次のトークン
            //     //printf("  sub rax, %d\n", expect_number());
            //     Console.Write($"  mov rax, {expect_number(token,ref curIndex)}\n" );
            // }
            
            Console.Write("  ret\n");
            return 0;
        }
        
        // 入力文字列pをトークナイズしてそれを返す
        // Token *tokenize(char *p) {
        //     Token head;
        //     head.next = NULL;
        //     Token *cur = &head;

        //     while (*p) {
        //         // 空白文字をスキップ
        //         if (isspace(*p)) {
        //         p++;
        //         continue;
        //         }

        //         if (*p == '+' || *p == '-') {
        //         cur = new_token(TK_RESERVED, cur, p++);
        //         continue;
        //         }

        //         if (isdigit(*p)) {
        //         cur = new_token(TK_NUM, cur, p);
        //         cur->val = strtol(p, &p, 10);
        //         continue;
        //         }

        //         error("トークナイズできません");
        //     }

        //     new_token(TK_EOF, cur, p);
        //     return head.next;
        // }
        static List<Token> tokenize(string p) {
            int next = 1;
            Token cur = new();
            List<Token> tokenList =new List<Token>();

            var cs = p.ToCharArray();
            int i= 0;
            while(i<cs.Length) {
                // 空白文字をスキップ
                if (isspace(cs[i].ToString())) {
                    i++;
                    continue;
                }

                if (isarithmetic(cs[i].ToString())) {
                    cur = new_token(TokenKind.TK_RESERVED, next, cs[i].ToString());
                    tokenList.Add(cur);
                    next++;
                    i++;
                    continue;
                }

                if (isdigit(cs[i].ToString())) {
        
                    string numberStr = cs[i].ToString();
                    i++;
                    if(i<cs.Length){
                        while(isdigit(cs[i].ToString())) {
                            numberStr+= cs[i].ToString();
                            i++;
                            if(i>=cs.Length)break;
                        }
                    }
                    cur = new_token(TokenKind.TK_NUM,next, numberStr);
                    cur.val = strtol(numberStr);
                    tokenList.Add(cur);
                    next++;
                    continue;
                }

                //ここに進んだら例外
                error("トークナイズできません");
            }

            cur = new_token(TokenKind.TK_EOF, next, "");
            tokenList.Add(cur);
            return tokenList;
        }

        // 抽象構文木を下りながらコード生成
        // void gen(Node *node) {
        //   if (node->kind == ND_NUM) {
        //     printf("  push %d\n", node->val);
        //     return;
        //   }

        //   gen(node->lhs);
        //   gen(node->rhs);

        //   printf("  pop rdi\n");
        //   printf("  pop rax\n");

        //   switch (node->kind) {
        //   case ND_ADD:
        //     printf("  add rax, rdi\n");
        //     break;
        //   case ND_SUB:
        //     printf("  sub rax, rdi\n");
        //     break;
        //   case ND_MUL:
        //     printf("  imul rax, rdi\n");
        //     break;
        //   case ND_DIV:
        //     printf("  cqo\n");
        //     printf("  idiv rdi\n");
        //     break;
        //   }

        //   printf("  push rax\n");
        // }
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
            }

            Console.Write("  push rax\n");
        }

        // パースする
        // Node *expr() {
        //   Node *node = mul();

        //   for (;;) {
        //     if (consume('+'))
        //       node = new_node(ND_ADD, node, mul());
        //     else if (consume('-'))
        //       node = new_node(ND_SUB, node, mul());
        //     else
        //       return node;
        //   }
        // }
        static Node expr(List<Token> tokenList,ref int curIndex) {
            Node node = mul(tokenList,ref curIndex);
            Token token = getToken(tokenList,curIndex);//次のトークン
            for (;;) {
                if (consume(token,"+",ref curIndex)){
                    node = new_node(NodeKind.ND_ADD, node, mul(tokenList,ref curIndex));
                    token = getToken(tokenList,curIndex);//次のトークン
                    }
                else if (consume(token,"-",ref curIndex)){
                    node = new_node(NodeKind.ND_SUB, node, mul(tokenList,ref curIndex));
                    token = getToken(tokenList,curIndex);//次のトークン
                    }
                else
                    return node;
            }
        }

        // Node *mul() {
        //   Node *node = primary();

        //   for (;;) {
        //     if (consume('*'))
        //       node = new_node(ND_MUL, node, primary());
        //     else if (consume('/'))
        //       node = new_node(ND_DIV, node, primary());
        //     else
        //       return node;
        //   }
        // }
        static Node mul(List<Token> tokenList,ref int curIndex) {
            Node node = primary(tokenList,ref curIndex);
            Token token = getToken(tokenList,curIndex);//次のトークン
            for (;;) {
                if (consume(token,"*",ref curIndex)){
                    node = new_node(NodeKind.ND_MUL, node, primary(tokenList,ref curIndex));
                    token = getToken(tokenList,curIndex);//次のトークン
                }
                else if (consume(token,"/",ref curIndex)){
                    node = new_node(NodeKind.ND_DIV, node, primary(tokenList,ref curIndex));
                    token = getToken(tokenList,curIndex);//次のトークン
                }
                else
                return node;
            }
        }

        // Node *primary() {
        //   // 次のトークンが"("なら、"(" expr ")"のはず
        //   if (consume('(')) {
        //     Node *node = expr();
        //     expect(')');
        //     return node;
        //   }

        //   // そうでなければ数値のはず
        //   return new_node_num(expect_number());
        // }
        static Node primary(List<Token> tokenList,ref int curIndex) {
            Token token = getToken(tokenList,curIndex);//次のトークン
            // 次のトークンが"("なら、"(" expr ")"のはず
            if (consume(token,"(",ref curIndex)) {
                Node node = expr(tokenList,ref curIndex);
                token = getToken(tokenList,curIndex);//次のトークン
                expect(token,")",ref curIndex);
                token = getToken(tokenList,curIndex);//次のトークン
                return node;
            }

            // そうでなければ数値のはず
            return new_node_num(expect_number(token,ref curIndex));
        }

        static Token getToken(List<Token> tokenList,int curIndex) {
            if(curIndex>=tokenList.Count) return new Token();
            Token token = tokenList.ElementAt(curIndex);//次のトークン
            return token;
        }
        // Node *new_node(NodeKind kind, Node *lhs, Node *rhs) {
        //   Node *node = calloc(1, sizeof(Node));
        //   node->kind = kind;
        //   node->lhs = lhs;
        //   node->rhs = rhs;
        //   return node;
        // }
        static Node new_node(NodeKind kind, Node lhs, Node rhs) {
            Node node = new();
            node.kind = kind;
            node.lhs = lhs;
            node.rhs = rhs;
            return node;
        }

        // Node *new_node_num(int val) {
        //   Node *node = calloc(1, sizeof(Node));
        //   node->kind = ND_NUM;
        //   node->val = val;
        //   return node;
        // }
        static Node new_node_num(int val) {
            Node node = new();
            node.kind = NodeKind.ND_NUM;
            node.val = val;
            return node;
        }
        // エラーを報告するための関数
        // printfと同じ引数を取る
        // void error(char *fmt, ...) {
        //     va_list ap;
        //     va_start(ap, fmt);
        //     vfprintf(stderr, fmt, ap);
        //     fprintf(stderr, "\n");
        //     exit(1);
        // }
        static void error(string fmt) {
            Console.WriteLine(fmt);
        }
        // 次のトークンが期待している記号のときには、トークンを1つ読み進めて
        // 真を返す。それ以外の場合には偽を返す。
        // bool consume(char op) {
        // if (token->kind != TK_RESERVED || token->str[0] != op)
        //     return false;
        // token = token->next;
        // return true;
        // }
        static bool consume(Token token,string op, ref int next) {
            if (token.kind != TokenKind.TK_RESERVED || token.str != op)
                return false;
            next = token.next;
            return true;
        }
        // 次のトークンが期待している記号のときには、トークンを1つ読み進める。
        // それ以外の場合にはエラーを報告する。
        // void expect(char op) {
        //     if (token->kind != TK_RESERVED || token->str[0] != op)
        //         error("'%c'ではありません", op);
        //     token = token->next;
        // }
       static  void expect(Token token,string op, ref int next) {
            if (token.kind != TokenKind.TK_RESERVED || token.str != op)
                error($"'{op}'ではありません");
            next = token.next;
        }

        // 次のトークンが数値の場合、トークンを1つ読み進めてその数値を返す。
        // それ以外の場合にはエラーを報告する。
        // int expect_number() {
        //     if (token->kind != TK_NUM)
        //         error("数ではありません");
        //     int val = token->val;
        //     token = token->next;
        //     return val;
        // }
        static int expect_number(Token token, ref int next) {
            if (token.kind != TokenKind.TK_NUM)
                error("数ではありません");
            int val = token.val;
            next = token.next;
            return val;
        }

        static bool at_eof(Token token) {
            return token.kind == TokenKind.TK_EOF;
        }

        // 新しいトークンを作成してcurに繋げる
        // Token *new_token(TokenKind kind, Token *cur, char *str) {
        //     Token *tok = calloc(1, sizeof(Token));
        //     tok->kind = kind;
        //     tok->str = str;
        //     cur->next = tok;
        //     return tok;
        // }
        static Token new_token(TokenKind kind, int next, string str) {
            Token tok = new();
            tok.kind = kind;
            tok.str = str;
            tok.next = next;
            return tok;
        }
        static bool isspace( string input )
        {
             return( Regex.IsMatch( input,"\\s" ) );
        }
        static bool isarithmetic( string input )
        {
             return( Regex.IsMatch( input,"[\\+\\-\\*\\/\\(\\)]" ) );
        }
        static bool isdigit( string input )
        {
             return( Regex.IsMatch( input, "[0-9]" ) );
        }
        static int strtol(string str){
            return  int.Parse(str);
        }

        
    }
}